using TravelMate.Mobile.Models;

namespace TravelMate.Mobile.Services;

public static class OfflineStoryCatalog
{
    private static readonly OfflineStory[] Stories =
    [
        new("Charminar", 17.3616, 78.4747, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd204"), "The Four Minarets at the Heart of Hyderabad", "Charminar is a 1591 monument and mosque in Hyderabad, widely treated as one of the city's defining symbols.", ["history", "architecture", "culture"]),
        new("Makkah Masjid", 17.3600, 78.4730, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd211"), "The Granite Mosque Beside Charminar", "Makkah Masjid is one of Hyderabad's largest and oldest mosques, close to Charminar in the Old City.", ["religion", "history", "architecture"]),
        new("Salar Jung Museum", 17.3713, 78.4804, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd208"), "One Collection, Many Worlds", "Salar Jung Museum preserves a major collection of art, manuscripts, sculpture, textiles, and historic objects.", ["museum", "art", "history"]),
        new("Hussain Sagar", 17.4239, 78.4738, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd206"), "A Lake Between Hyderabad and Secunderabad", "Hussain Sagar is a large heart-shaped lake known for its waterfront setting and Buddha statue.", ["nature", "waterfront", "culture"]),
        new("Birla Mandir, Hyderabad", 17.4062, 78.4691, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd209"), "White Marble Above the City", "Birla Mandir is a Hindu temple on Naubath Pahad, known for city views and white marble architecture.", ["religion", "architecture", "viewpoint"]),
        new("Golconda Fort", 17.3833, 78.4011, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd205"), "The Fort of Diamonds and Dynasties", "Golconda is a fortified citadel associated with the Qutb Shahi dynasty and the region's famous diamond trade.", ["history", "fort", "architecture"]),
        new("Qutb Shahi Tombs", 17.3950, 78.3968, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd210"), "The Royal Tomb Garden Near Golconda", "The Qutb Shahi Tombs form a historic garden of domed tombs and mosques near Golconda Fort.", ["history", "architecture", "heritage"]),
        new("Nandi Hills", 13.3702, 77.6835, Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd201"), "The Hill Fort Above Bengaluru", "Nandi Hills is known for old fort walls, temple history, sunrise views, and stories connected to Tipu Sultan's era.", ["history", "nature", "architecture"])
    ];

    public static IReadOnlyCollection<NearbyStoryDto> GetNearbyStories(
        double latitude,
        double longitude,
        double radiusMeters)
    {
        var radius = radiusMeters <= 0 ? 5_000 : radiusMeters;
        return Stories
            .Select(story => new
            {
                Story = story,
                DistanceMeters = DistanceToMeters(latitude, longitude, story.Latitude, story.Longitude)
            })
            .Where(item => item.DistanceMeters <= radius)
            .OrderBy(item => item.DistanceMeters)
            .Take(5)
            .Select(item => new NearbyStoryDto(
                item.Story.StoryId,
                item.Story.StoryId,
                item.Story.PlaceName,
                item.Story.Title,
                item.Story.ShortDescription,
                "en",
                Math.Round(item.DistanceMeters, 0),
                Math.Round(Math.Max(0.5, 1 - item.DistanceMeters / 10_000), 4),
                null,
                "Offline demo catalog"))
            .ToArray();
    }

    private static double DistanceToMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusMeters = 6_371_000;
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
            * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private sealed record OfflineStory(
        string PlaceName,
        double Latitude,
        double Longitude,
        Guid StoryId,
        string Title,
        string ShortDescription,
        string[] Categories);
}
