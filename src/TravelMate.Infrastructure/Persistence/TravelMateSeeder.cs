using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TravelMate.Infrastructure.Persistence;

public sealed class TravelMateSeeder(TravelMateDbContext dbContext)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Guid NandiHillsId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311101");
    private static readonly Guid CricketStadiumId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311102");
    private static readonly Guid StLuciaId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311103");
    private static readonly Guid CharminarId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311104");
    private static readonly Guid GolcondaFortId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311105");
    private static readonly Guid HussainSagarId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311106");
    private static readonly Guid ChowmahallaPalaceId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311107");
    private static readonly Guid SalarJungMuseumId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311108");
    private static readonly Guid BirlaMandirId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311109");

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsRelational())
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }

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
            },
            new PlaceEntity
            {
                Id = CharminarId,
                Name = "Charminar",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3616,
                Longitude = 78.4747,
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "architecture", "culture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = GolcondaFortId,
                Name = "Golconda Fort",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3833,
                Longitude = 78.4011,
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "fort", "architecture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = HussainSagarId,
                Name = "Hussain Sagar",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.4239,
                Longitude = 78.4738,
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "waterfront", "culture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = ChowmahallaPalaceId,
                Name = "Chowmahalla Palace",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3578,
                Longitude = 78.4717,
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "palace", "architecture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = SalarJungMuseumId,
                Name = "Salar Jung Museum",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3713,
                Longitude = 78.4804,
                CategoriesJson = JsonSerializer.Serialize(new[] { "museum", "art", "history" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = BirlaMandirId,
                Name = "Birla Mandir, Hyderabad",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.4062,
                Longitude = 78.4691,
                CategoriesJson = JsonSerializer.Serialize(new[] { "religion", "architecture", "viewpoint" }, JsonOptions)
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
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd204"),
                PlaceId = CharminarId,
                Title = "The Four Minarets at the Heart of Hyderabad",
                ShortDescription = "Charminar is a 1591 monument and mosque in Hyderabad, widely treated as one of the city's defining symbols and part of the Qutb Shahi heritage landscape.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "architecture", "culture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Charminar",
                QualityScore = 90
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd205"),
                PlaceId = GolcondaFortId,
                Title = "The Fort of Diamonds and Dynasties",
                ShortDescription = "Golconda is a fortified citadel on Hyderabad's western side, associated with the Qutb Shahi dynasty and the region's famous diamond trade and monumental Deccan architecture.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "fort", "architecture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Golconda",
                QualityScore = 89
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd206"),
                PlaceId = HussainSagarId,
                Title = "A Lake Between Hyderabad and Secunderabad",
                ShortDescription = "Hussain Sagar is a large heart-shaped lake built during the Qutb Shahi period, now known for its waterfront setting and the prominent Buddha statue on Gibraltar Rock.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "waterfront", "culture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Hussain_Sagar",
                QualityScore = 84
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd207"),
                PlaceId = ChowmahallaPalaceId,
                Title = "The Nizams' Ceremonial Palace",
                ShortDescription = "Chowmahalla Palace was the palace of Hyderabad's Nizams, close to Charminar, and its museum setting tells the story of royal ceremony, court life, and Asaf Jahi rule.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "palace", "architecture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Chowmahalla_Palace",
                QualityScore = 86
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd208"),
                PlaceId = SalarJungMuseumId,
                Title = "One Collection, Many Worlds",
                ShortDescription = "Salar Jung Museum stands on the southern bank of the Musi River and preserves a major collection of art, manuscripts, sculpture, textiles, and objects associated with Salar Jung III.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "museum", "art", "history" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Salar_Jung_Museum",
                QualityScore = 87
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd209"),
                PlaceId = BirlaMandirId,
                Title = "White Marble Above the City",
                ShortDescription = "Birla Mandir is a Hindu temple built on Naubath Pahad, a hillock in Hyderabad, known for its elevated city views and white marble temple architecture.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "religion", "architecture", "viewpoint" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Birla_Mandir,_Hyderabad",
                QualityScore = 82
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
