using TravelMate.Domain;

namespace TravelMate.Application;

public sealed class ConversationService(NearbyStoryService nearbyStoryService)
{
    public async Task<ConversationResponse> ReplyAsync(
        ConversationRequest request,
        CancellationToken cancellationToken)
    {
        var nearby = await nearbyStoryService.GetNearbyStoriesAsync(
            request.UserId,
            new GeoPoint(request.Latitude, request.Longitude),
            request.RadiusMeters,
            cancellationToken);

        var topStory = nearby.FirstOrDefault();
        if (topStory is null)
        {
            return new ConversationResponse(
                "I could not find a nearby story yet. Try increasing the radius or adding pilot content for this route.",
                null);
        }

        return new ConversationResponse(
            $"You are near {topStory.PlaceName}. {topStory.ShortDescription}",
            topStory.StoryId);
    }
}

public sealed record ConversationRequest(
    string UserId,
    string Message,
    double Latitude,
    double Longitude,
    double RadiusMeters = 5_000);

public sealed record ConversationResponse(string Reply, Guid? SuggestedStoryId);
