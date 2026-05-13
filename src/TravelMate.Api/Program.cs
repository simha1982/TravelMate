using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure;
using TravelMate.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var azureAdB2CEnabled = builder.Configuration.GetValue("AzureAdB2C:Enabled", false)
    && !string.IsNullOrWhiteSpace(builder.Configuration["AzureAdB2C:Authority"])
    && !string.IsNullOrWhiteSpace(builder.Configuration["AzureAdB2C:Audience"]);
var adminAuthEnabled = builder.Configuration.GetValue("AdminAuth:RequireAuthorization", false);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}
if (azureAdB2CEnabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["AzureAdB2C:Authority"];
            options.Audience = builder.Configuration["AzureAdB2C:Audience"];
        });
}
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddTravelMateInfrastructure(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddSingleton(
    builder.Configuration.GetSection("AiAudit").Get<AiAuditOptions>() ?? new AiAuditOptions());
builder.Services.AddScoped<IModelGateway>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var options = configuration.GetSection("AzureOpenAI").Get<AzureOpenAiOptions>() ?? new AzureOpenAiOptions();
    IModelGateway inner;
    if (!options.IsConfigured)
    {
        inner = new StubModelGateway();
    }
    else
    {
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        inner = new AzureOpenAiModelGateway(httpClientFactory.CreateClient("AzureOpenAI"), options);
    }

    var auditRepository = serviceProvider.GetRequiredService<IAiAuditRepository>();
    var auditOptions = serviceProvider.GetRequiredService<AiAuditOptions>();
    return new AuditedModelGateway(inner, auditRepository, auditOptions);
});
builder.Services.AddSingleton<ITextToSpeechGateway>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var options = configuration.GetSection("AzureSpeech").Get<AzureSpeechOptions>() ?? new AzureSpeechOptions();
    if (!options.IsConfigured)
    {
        return new StubTextToSpeechGateway();
    }

    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    return new AzureSpeechGateway(httpClientFactory.CreateClient("AzureSpeech"), options);
});
builder.Services.AddScoped<NearbyStoryService>();
builder.Services.AddScoped<ConversationService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<ContributionService>();
builder.Services.AddScoped<EntitlementService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<TravelMateDatabaseInitializer>();
    await initializer.InitializeAsync(CancellationToken.None);
    await SeedSearchAsync(scope.ServiceProvider, CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    var requireApiKey = app.Configuration.GetValue("Auth:RequireApiKey", false);
    var configuredApiKey = app.Configuration["Auth:ApiKey"];
    if (!requireApiKey)
    {
        await next();
        return;
    }

    if (string.IsNullOrWhiteSpace(configuredApiKey))
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("API key auth is enabled but Auth:ApiKey is not configured.");
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-TravelMate-Api-Key", out var providedApiKey)
        || providedApiKey != configuredApiKey)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Missing or invalid TravelMate API key.");
        return;
    }

    await next();
});

if (azureAdB2CEnabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGet("/", () => Results.Ok(new
{
    name = "TravelMate",
    status = "running",
    description = "Location-aware travel storytelling API",
    endpoints = new[]
    {
        "GET /api/stories/nearby",
        "GET /api/stories/{storyId}",
        "GET /api/preferences/{userId}",
        "POST /api/preferences",
        "GET /api/users/{userId}/consents",
        "POST /api/users/{userId}/consents",
        "POST /api/stories/{storyId}/playback-events",
        "POST /api/conversation/message",
        "GET /api/subscriptions/{userId}/entitlements",
        "POST /api/admin/subscriptions",
        "GET /api/admin/ai-audit"
    }
}));

app.MapGet("/api/auth/status", () => Results.Ok(new
{
    apiKeyEnabled = app.Configuration.GetValue("Auth:RequireApiKey", false),
    azureAdB2CEnabled,
    adminAuthEnabled
}))
.WithName("AuthStatus");

app.MapGet("/api/mobile/diagnostics", (
    IStorySearchService searchService,
    IConfiguration configuration) =>
{
    var assemblyVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "dev";
    return Results.Ok(new MobileDiagnosticsResponse(
        "TravelMate",
        assemblyVersion,
        DateTimeOffset.UtcNow,
        app.Environment.EnvironmentName,
        app.Configuration.GetValue("Auth:RequireApiKey", false),
        azureAdB2CEnabled,
        adminAuthEnabled,
        searchService.GetType().Name.Contains("Azure", StringComparison.OrdinalIgnoreCase)
            ? "azure-configured"
            : "local",
        string.IsNullOrWhiteSpace(configuration["AudioStorage:ConnectionString"]) ? "local" : "azure-configured",
        string.IsNullOrWhiteSpace(configuration["AzureOpenAI:Endpoint"]) ? "local-stub" : "azure-configured",
        string.IsNullOrWhiteSpace(configuration["AzureSpeech:Region"]) ? "local-stub" : "azure-configured"));
})
.WithName("MobileDiagnostics");

app.MapGet("/health", async (
    ITravelMateRepository repository,
    IStorySearchService searchService,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var checks = new List<HealthCheckStatus>();
    var databaseHealthy = false;
    try
    {
        var places = await repository.GetPlacesAsync(cancellationToken);
        databaseHealthy = places.Count >= 0;
    }
    catch (Exception ex)
    {
        checks.Add(new HealthCheckStatus("database", "unhealthy", ex.Message));
    }

    if (databaseHealthy)
    {
        checks.Add(new HealthCheckStatus("database", "healthy"));
    }

    checks.Add(new HealthCheckStatus(
        "search",
        searchService.GetType().Name.Contains("Azure", StringComparison.OrdinalIgnoreCase)
            ? "azure-configured"
            : "local"));
    checks.Add(new HealthCheckStatus(
        "storage",
        string.IsNullOrWhiteSpace(configuration["AudioStorage:ConnectionString"]) ? "local" : "azure-configured"));
    checks.Add(new HealthCheckStatus(
        "ai",
        string.IsNullOrWhiteSpace(configuration["AzureOpenAI:Endpoint"]) ? "local-stub" : "azure-configured"));

    var healthy = checks.All(check => !check.Status.Equals("unhealthy", StringComparison.OrdinalIgnoreCase));
    return healthy ? Results.Ok(new { status = "healthy", checks }) : Results.Problem("One or more checks failed.", statusCode: 503);
})
.WithName("Health");

var adminApi = app.MapGroup("/api/admin");
if (adminAuthEnabled)
{
    adminApi.RequireAuthorization("AdminOnly");
}

app.MapGet("/api/stories/nearby", async (
    string userId,
    double lat,
    double lon,
    double radiusMeters,
    NearbyStoryService service,
    CancellationToken cancellationToken) =>
{
    var stories = await service.GetNearbyStoriesAsync(
        userId,
        new GeoPoint(lat, lon),
        radiusMeters <= 0 ? 5_000 : radiusMeters,
        cancellationToken);

    return Results.Ok(stories);
})
.WithName("GetNearbyStories");

app.MapGet("/api/places", async (
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var places = await repository.GetPlacesAsync(cancellationToken);
    return Results.Ok(places);
})
.WithName("GetPlaces");

app.MapGet("/api/stories", async (
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var stories = await repository.GetStoriesAsync(cancellationToken);
    return Results.Ok(stories);
})
.WithName("GetStories");

app.MapGet("/api/stories/{storyId:guid}", async (
    Guid storyId,
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var stories = await repository.GetStoriesAsync(cancellationToken);
    var story = stories.FirstOrDefault(item => item.Id == storyId);
    if (story is null)
    {
        return Results.NotFound();
    }

    var places = await repository.GetPlacesAsync(cancellationToken);
    var place = places.FirstOrDefault(item => item.Id == story.PlaceId);
    return Results.Ok(new StoryDetailResponse(story, place));
})
.WithName("GetStoryDetail");

app.MapGet("/api/preferences/{userId}", async (
    string userId,
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var preference = await repository.GetPreferenceAsync(userId, cancellationToken);
    return Results.Ok(preference);
})
.WithName("GetPreferences");

app.MapPost("/api/preferences", async (
    UserPreference preference,
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    await repository.SavePreferenceAsync(preference, cancellationToken);
    return Results.NoContent();
})
.WithName("SavePreferences");

app.MapGet("/api/users/{userId}/consents", async (
    string userId,
    IUserConsentRepository repository,
    CancellationToken cancellationToken) =>
{
    var consent = await repository.GetAsync(userId, cancellationToken);
    return Results.Ok(consent);
})
.WithName("GetUserConsents");

app.MapPost("/api/users/{userId}/consents", async (
    string userId,
    SaveUserConsentRequest request,
    IUserConsentRepository repository,
    CancellationToken cancellationToken) =>
{
    var consent = new UserConsent(
        userId,
        request.LocationConsent,
        request.VoiceConsent,
        request.PersonalizationConsent,
        DateTimeOffset.UtcNow);

    await repository.SaveAsync(consent, cancellationToken);
    return Results.Ok(consent);
})
.WithName("SaveUserConsents");

app.MapPost("/api/stories/{storyId:guid}/playback-events", async (
    Guid storyId,
    PlaybackEventRequest request,
    ITravelMateRepository repository,
    EntitlementService entitlementService,
    CancellationToken cancellationToken) =>
{
    if (request.Action is PlaybackAction.Played or PlaybackAction.Completed)
    {
        var entitlement = await entitlementService.GetEntitlementsAsync(request.UserId, cancellationToken);
        if (!entitlement.CanPlayMoreStories)
        {
            return Results.Problem(
                "Daily free story limit reached. Upgrade the subscription to continue playback.",
                statusCode: StatusCodes.Status402PaymentRequired);
        }
    }

    var playbackEvent = new PlaybackEvent(
        request.UserId,
        storyId,
        request.Action,
        DateTimeOffset.UtcNow);

    await repository.SavePlaybackEventAsync(playbackEvent, cancellationToken);
    return Results.Accepted($"/api/stories/{storyId}", playbackEvent);
})
.WithName("SavePlaybackEvent");

app.MapGet("/api/subscriptions/{userId}/entitlements", async (
    string userId,
    EntitlementService entitlementService,
    CancellationToken cancellationToken) =>
{
    var entitlement = await entitlementService.GetEntitlementsAsync(userId, cancellationToken);
    return Results.Ok(entitlement);
})
.WithName("GetEntitlements");

adminApi.MapPost("/subscriptions", async (
    SaveSubscriptionRequest request,
    EntitlementService entitlementService,
    CancellationToken cancellationToken) =>
{
    var subscription = await entitlementService.SaveAsync(request, cancellationToken);
    return Results.Ok(subscription);
})
.WithName("SaveSubscription");

adminApi.MapGet("/ai-audit", async (
    int? limit,
    IAiAuditRepository repository,
    CancellationToken cancellationToken) =>
{
    var events = await repository.GetRecentAsync(limit ?? 50, cancellationToken);
    return Results.Ok(events);
})
.WithName("GetAiAuditEvents");

adminApi.MapPost("/places", async (
    SavePlaceRequest request,
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var place = await repository.SavePlaceAsync(request, cancellationToken);
    return Results.Created($"/api/places/{place.Id}", place);
})
.WithName("CreatePlace");

adminApi.MapPut("/places/{placeId:guid}", async (
    Guid placeId,
    SavePlaceRequest request,
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var place = await repository.SavePlaceAsync(request with { Id = placeId }, cancellationToken);
    return Results.Ok(place);
})
.WithName("UpdatePlace");

adminApi.MapPost("/stories", async (
    SaveStoryRequest request,
    ITravelMateRepository repository,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken) =>
{
    var story = await repository.SaveStoryAsync(request, cancellationToken);
    await SeedSearchAsync(serviceProvider, cancellationToken);
    return Results.Created($"/api/stories/{story.Id}", story);
})
.WithName("CreateStory");

adminApi.MapPut("/stories/{storyId:guid}", async (
    Guid storyId,
    SaveStoryRequest request,
    ITravelMateRepository repository,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken) =>
{
    var story = await repository.SaveStoryAsync(request with { Id = storyId }, cancellationToken);
    await SeedSearchAsync(serviceProvider, cancellationToken);
    return Results.Ok(story);
})
.WithName("UpdateStory");

app.MapPost("/api/conversation/message", async (
    ConversationRequest request,
    ConversationService conversationService,
    CancellationToken cancellationToken) =>
{
    var response = await conversationService.ReplyAsync(request, cancellationToken);
    return Results.Ok(response);
})
.WithName("ConversationMessage");

app.MapPost("/api/ai/story-summary", async (
    StorySummaryRequest request,
    IModelGateway modelGateway,
    CancellationToken cancellationToken) =>
{
    var response = await modelGateway.CompleteJsonAsync<StorySummaryResponse>(
        "travel-story-summary",
        request,
        cancellationToken);

    return Results.Ok(response);
})
.WithName("GenerateStorySummary");

app.MapPost("/api/speech/synthesize", async (
    SpeechSynthesisRequest request,
    ITextToSpeechGateway speechGateway,
    CancellationToken cancellationToken) =>
{
    var audio = await speechGateway.SynthesizeAsync(
        request.Text,
        request.LanguageCode,
        request.VoiceName,
        cancellationToken);

    return Results.File(audio.Content, audio.ContentType, $"travelmate-story.{audio.FileExtension}");
})
.WithName("SynthesizeSpeech");

app.MapPost("/api/stories/{storyId:guid}/audio", async (
    Guid storyId,
    SpeechSynthesisRequest request,
    ITextToSpeechGateway speechGateway,
    IAudioStorageService audioStorage,
    CancellationToken cancellationToken) =>
{
    var audio = await speechGateway.SynthesizeAsync(
        request.Text,
        request.LanguageCode,
        request.VoiceName,
        cancellationToken);
    var stored = await audioStorage.SaveStoryAudioAsync(
        storyId,
        audio.Content,
        audio.ContentType,
        audio.FileExtension,
        cancellationToken);

    return Results.Ok(stored);
})
.WithName("GenerateAndStoreStoryAudio");

app.MapPost("/api/search/reindex", async (
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken) =>
{
    await SeedSearchAsync(serviceProvider, cancellationToken);
    return Results.Accepted();
})
.WithName("ReindexStories");

app.MapPost("/api/rag/answer", async (
    RagAnswerRequest request,
    RagService ragService,
    CancellationToken cancellationToken) =>
{
    var response = await ragService.AnswerAsync(request, cancellationToken);
    return Results.Ok(response);
})
.WithName("RagAnswer");

app.MapPost("/api/contributions", async (
    SubmitContributionRequest request,
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var contribution = await contributionService.SubmitAsync(request, cancellationToken);
    return Results.Created($"/api/contributions/{contribution.Id}", contribution);
})
.WithName("SubmitContribution");

adminApi.MapGet("/moderation-queue", async (
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var queue = await contributionService.GetQueueAsync(cancellationToken);
    return Results.Ok(queue);
})
.WithName("ModerationQueue");

adminApi.MapGet("/contributions/{contributionId:guid}/moderation-results", async (
    Guid contributionId,
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var results = await contributionService.GetModerationResultsAsync(contributionId, cancellationToken);
    return Results.Ok(results);
})
.WithName("ModerationResults");

adminApi.MapPost("/contributions/{contributionId:guid}/approve", async (
    Guid contributionId,
    ContributionService contributionService,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken) =>
{
    var updated = await contributionService.ApproveAndPublishAsync(contributionId, cancellationToken);
    if (updated is not null)
    {
        await SeedSearchAsync(serviceProvider, cancellationToken);
    }

    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.WithName("ApproveContribution");

adminApi.MapPost("/contributions/{contributionId:guid}/reject", async (
    Guid contributionId,
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var updated = await contributionService.UpdateStatusAsync(contributionId, ContributionStatus.Rejected, cancellationToken);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.WithName("RejectContribution");

app.Run();

static async Task SeedSearchAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
{
    var repository = serviceProvider.GetRequiredService<ITravelMateRepository>();
    var searchService = serviceProvider.GetRequiredService<IStorySearchService>();
    var places = await repository.GetPlacesAsync(cancellationToken);
    var stories = await repository.GetStoriesAsync(cancellationToken);
    var documents = stories.Select(story =>
    {
        var place = places.FirstOrDefault(item => item.Id == story.PlaceId);
        return new SearchableStory(
            story.Id.ToString("N"),
            place?.Name ?? "Unknown place",
            story.Title,
            story.ShortDescription,
            story.LanguageCode,
            story.Categories.ToArray(),
            story.SourceName,
            story.SourceUrl);
    }).ToArray();

    await searchService.IndexStoriesAsync(documents, cancellationToken);
}

public sealed record PlaybackEventRequest(string UserId, PlaybackAction Action);

public sealed record HealthCheckStatus(string Name, string Status, string? Detail = null);

public sealed record StoryDetailResponse(Story Story, Place? Place);

public sealed record MobileDiagnosticsResponse(
    string AppName,
    string ApiVersion,
    DateTimeOffset ServerUtc,
    string EnvironmentName,
    bool ApiKeyEnabled,
    bool AzureAdB2CEnabled,
    bool AdminAuthEnabled,
    string SearchMode,
    string StorageMode,
    string AiMode,
    string SpeechMode);

public partial class Program;
