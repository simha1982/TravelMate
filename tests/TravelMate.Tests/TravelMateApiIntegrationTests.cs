using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TravelMate.Tests;

public sealed class TravelMateApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public TravelMateApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Database:Provider", "InMemory");
            builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
            builder.UseSetting("AdminAuth:RequireAuthorization", "false");
        }).CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsReadinessChecks()
    {
        var response = await client.GetAsync("/health");
        var health = await response.Content.ReadFromJsonAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("healthy", health?.Status);
        Assert.NotEmpty(health?.Checks ?? []);
    }

    [Fact]
    public async Task NearbyStories_ReturnsHyderabadSeedStories()
    {
        var stories = await client.GetFromJsonAsync<NearbyStoryResponse[]>(
            "/api/stories/nearby?userId=test-user&lat=17.3616&lon=78.4747&radiusMeters=15000");

        Assert.NotNull(stories);
        Assert.Contains(stories, story => story.PlaceName.Contains("Charminar", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AdminCreateStory_AddsStoryToPublicStoryList()
    {
        var places = await client.GetFromJsonAsync<PlaceResponse[]>("/api/places") ?? [];
        var place = Assert.Single(
            places,
            item => item.Name.Contains("Charminar", StringComparison.OrdinalIgnoreCase));
        var title = $"Pilot integration story {Guid.NewGuid():N}";

        using var response = await client.PostAsJsonAsync("/api/admin/stories", new
        {
            placeId = place.Id,
            title,
            shortDescription = "A test story created through the admin content API.",
            languageCode = "en",
            categories = new[] { "test", "pilot" },
            sourceName = "Integration test",
            sourceUrl = "https://example.com/travelmate-test",
            audioUrl = (string?)null,
            qualityScore = 50
        });

        response.EnsureSuccessStatusCode();
        var stories = await client.GetFromJsonAsync<StoryResponse[]>("/api/stories") ?? [];

        Assert.Contains(stories, story => story.Title == title);
    }

    private sealed record HealthResponse(string Status, HealthCheckResponse[] Checks);
    private sealed record HealthCheckResponse(string Name, string Status, string? Detail);
    private sealed record PlaceResponse(Guid Id, string Name);
    private sealed record StoryResponse(Guid Id, string Title);
    private sealed record NearbyStoryResponse(Guid StoryId, string PlaceName, string Title);
}
