using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<ITravelMateRepository, InMemoryTravelMateRepository>();
builder.Services.AddSingleton<IModelGateway, StubModelGateway>();
builder.Services.AddScoped<NearbyStoryService>();
builder.Services.AddScoped<ConversationService>();

var app = builder.Build();

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

app.Run();

public sealed record PlaybackEventRequest(string UserId, PlaybackAction Action);
