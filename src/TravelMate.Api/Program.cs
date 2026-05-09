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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<TravelMateSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

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

app.Run();

public sealed record PlaybackEventRequest(string UserId, PlaybackAction Action);
