using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure;
using TravelMate.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}
builder.Services.AddTravelMateInfrastructure(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IModelGateway>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var options = configuration.GetSection("AzureOpenAI").Get<AzureOpenAiOptions>() ?? new AzureOpenAiOptions();
    if (!options.IsConfigured)
    {
        return new StubModelGateway();
    }

    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    return new AzureOpenAiModelGateway(httpClientFactory.CreateClient("AzureOpenAI"), options);
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<TravelMateSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
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

app.MapGet("/", () => Results.Ok(new
{
    name = "TravelMate",
    status = "running",
    description = "Location-aware travel storytelling API",
    endpoints = new[]
    {
        "GET /api/stories/nearby",
        "GET /api/preferences/{userId}",
        "POST /api/preferences",
        "POST /api/stories/{storyId}/playback-events",
        "POST /api/conversation/message"
    }
}));

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

app.MapPost("/api/stories/{storyId:guid}/playback-events", async (
    Guid storyId,
    PlaybackEventRequest request,
    ITravelMateRepository repository,
    CancellationToken cancellationToken) =>
{
    var playbackEvent = new PlaybackEvent(
        request.UserId,
        storyId,
        request.Action,
        DateTimeOffset.UtcNow);

    await repository.SavePlaybackEventAsync(playbackEvent, cancellationToken);
    return Results.Accepted($"/api/stories/{storyId}", playbackEvent);
})
.WithName("SavePlaybackEvent");

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

app.MapGet("/api/admin/moderation-queue", async (
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var queue = await contributionService.GetQueueAsync(cancellationToken);
    return Results.Ok(queue);
})
.WithName("ModerationQueue");

app.MapGet("/api/admin/contributions/{contributionId:guid}/moderation-results", async (
    Guid contributionId,
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var results = await contributionService.GetModerationResultsAsync(contributionId, cancellationToken);
    return Results.Ok(results);
})
.WithName("ModerationResults");

app.MapPost("/api/admin/contributions/{contributionId:guid}/approve", async (
    Guid contributionId,
    ContributionService contributionService,
    CancellationToken cancellationToken) =>
{
    var updated = await contributionService.UpdateStatusAsync(contributionId, ContributionStatus.Approved, cancellationToken);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
})
.WithName("ApproveContribution");

app.MapPost("/api/admin/contributions/{contributionId:guid}/reject", async (
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
