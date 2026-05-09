using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TravelMate.Infrastructure.Persistence;

public sealed class TravelMateSeeder(TravelMateDbContext dbContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Guid NandiHillsId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311101");
    private static readonly Guid CricketStadiumId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311102");
    private static readonly Guid StLuciaId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311103");

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await dbContext.Places.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.Places.AddRange(
            new PlaceEntity
            {
                Id = NandiHillsId,
                Name = "Nandi Hills",
                Country = "India",
                Region = "Karnataka",
                Latitude = 13.3702,
                Longitude = 77.6835,
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "nature", "architecture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = CricketStadiumId,
                Name = "M. Chinnaswamy Stadium",
                Country = "India",
                Region = "Bengaluru",
                Latitude = 12.9788,
                Longitude = 77.5996,
                CategoriesJson = JsonSerializer.Serialize(new[] { "cricket", "sports", "culture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = StLuciaId,
                Name = "iSimangaliso Wetland Park",
                Country = "South Africa",
                Region = "KwaZulu-Natal",
                Latitude = -28.0000,
                Longitude = 32.4800,
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "scubaDiving", "history" }, JsonOptions)
            });

        dbContext.Stories.AddRange(
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd201"),
                PlaceId = NandiHillsId,
                Title = "The Hill Fort Above Bengaluru",
                ShortDescription = "Nandi Hills is known for its old fort walls, temple history, sunrise views, and stories connected to Tipu Sultan's era.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "nature", "architecture" }, JsonOptions),
                SourceName = "Curated pilot content",
                SourceUrl = "internal://pilot/nandi-hills",
                QualityScore = 88
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd202"),
                PlaceId = CricketStadiumId,
                Title = "A Cricket Landmark in the City",
                ShortDescription = "This Bengaluru stadium is a major cricket venue and a good example of the app recognizing a traveller's sports interests.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "cricket", "sports" }, JsonOptions),
                SourceName = "Curated pilot content",
                SourceUrl = "internal://pilot/chinnaswamy",
                QualityScore = 82
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd203"),
                PlaceId = StLuciaId,
                Title = "Wetlands, Coast, and Wildlife",
                ShortDescription = "This coastal wetland region is a strong pilot fit for nature, wildlife, and diving-related storytelling.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "scubaDiving" }, JsonOptions),
                SourceName = "Curated pilot content",
                SourceUrl = "internal://pilot/st-lucia",
                QualityScore = 84
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
