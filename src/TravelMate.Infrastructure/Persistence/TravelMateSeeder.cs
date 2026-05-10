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
    private static readonly Guid QutbShahiTombsId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311110");
    private static readonly Guid MakkahMasjidId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311111");
    private static readonly Guid FalaknumaPalaceId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311112");
    private static readonly Guid PuraniHaveliId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311113");
    private static readonly Guid NehruZooId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311114");
    private static readonly Guid PaigahTombsId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311115");
    private static readonly Guid RamojiFilmCityId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311116");
    private static readonly Guid ShilparamamId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311117");
    private static readonly Guid DurgamCheruvuId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311118");
    private static readonly Guid PaigahPalaceId = Guid.Parse("4d7df980-92e0-4ed7-a4a8-f98f82311119");

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
            },
            new PlaceEntity
            {
                Id = QutbShahiTombsId,
                Name = "Qutb Shahi Tombs",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3950,
                Longitude = 78.3968,
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "architecture", "heritage" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = MakkahMasjidId,
                Name = "Makkah Masjid",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3600,
                Longitude = 78.4730,
                CategoriesJson = JsonSerializer.Serialize(new[] { "religion", "history", "architecture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = FalaknumaPalaceId,
                Name = "Falaknuma Palace",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3315,
                Longitude = 78.4674,
                CategoriesJson = JsonSerializer.Serialize(new[] { "palace", "history", "architecture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = PuraniHaveliId,
                Name = "Purani Haveli",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3663,
                Longitude = 78.4820,
                CategoriesJson = JsonSerializer.Serialize(new[] { "palace", "museum", "history" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = NehruZooId,
                Name = "Nehru Zoological Park",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3506,
                Longitude = 78.4511,
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "wildlife", "family" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = PaigahTombsId,
                Name = "Paigah Tombs",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.3444,
                Longitude = 78.5084,
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "architecture", "heritage" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = RamojiFilmCityId,
                Name = "Ramoji Film City",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.2543,
                Longitude = 78.6808,
                CategoriesJson = JsonSerializer.Serialize(new[] { "film", "entertainment", "culture" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = ShilparamamId,
                Name = "Shilparamam",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.4526,
                Longitude = 78.3772,
                CategoriesJson = JsonSerializer.Serialize(new[] { "craft", "culture", "market" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = DurgamCheruvuId,
                Name = "Durgam Cheruvu",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.4344,
                Longitude = 78.3897,
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "lake", "cityscape" }, JsonOptions)
            },
            new PlaceEntity
            {
                Id = PaigahPalaceId,
                Name = "Paigah Palace",
                Country = "India",
                Region = "Hyderabad",
                Latitude = 17.4432,
                Longitude = 78.4622,
                CategoriesJson = JsonSerializer.Serialize(new[] { "palace", "history", "architecture" }, JsonOptions)
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
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd210"),
                PlaceId = QutbShahiTombsId,
                Title = "The Royal Tomb Garden Near Golconda",
                ShortDescription = "The Qutb Shahi Tombs form a historic garden of domed tombs and mosques for rulers of the Qutb Shahi dynasty, close to Golconda Fort.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "architecture", "heritage" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Qutb_Shahi_tombs",
                QualityScore = 88
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd211"),
                PlaceId = MakkahMasjidId,
                Title = "The Granite Mosque Beside Charminar",
                ShortDescription = "Makkah Masjid is one of Hyderabad's largest and oldest mosques, begun under Muhammad Qutb Shah and completed during Aurangzeb's reign.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "religion", "history", "architecture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Makkah_Masjid,_Hyderabad",
                QualityScore = 87
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd212"),
                PlaceId = FalaknumaPalaceId,
                Title = "The Palace Called Mirror of the Sky",
                ShortDescription = "Falaknuma Palace was built on a hillock by a Paigah nobleman and later owned by the Nizam of Hyderabad, known for its marble, halls, and skyward name.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "palace", "history", "architecture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Falaknuma_Palace",
                QualityScore = 86
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd213"),
                PlaceId = PuraniHaveliId,
                Title = "The Old Palace of Hyderabad",
                ShortDescription = "Purani Haveli, also called Masarrat Mahal palace, is an Old City palace linked with Hyderabad's Nizam-era heritage and museum landscape.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "palace", "museum", "history" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Purani_Haveli",
                QualityScore = 82
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd214"),
                PlaceId = NehruZooId,
                Title = "A City Zoo by Mir Alam Tank",
                ShortDescription = "Nehru Zoological Park, also called Zoo Park, is a major Hyderabad zoo near Mir Alam Tank with wildlife, family visits, and nature education.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "wildlife", "family" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Nehru_Zoological_Park",
                QualityScore = 80
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd215"),
                PlaceId = PaigahTombsId,
                Title = "The Marble Tombs of Paigah Nobles",
                ShortDescription = "Paigah Tombs preserve the funerary architecture of Hyderabad's Paigah nobility, a family closely tied to palaces, power, and Nizam-era court life.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "history", "architecture", "heritage" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Paigah_Tombs",
                QualityScore = 84
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd216"),
                PlaceId = RamojiFilmCityId,
                Title = "A Film Studio City Outside Hyderabad",
                ShortDescription = "Ramoji Film City is an integrated film studio facility outside Hyderabad, associated with large film sets, tourism, and the Ramoji media group.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "film", "entertainment", "culture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Ramoji_Film_City",
                QualityScore = 81
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd217"),
                PlaceId = ShilparamamId,
                Title = "The Crafts Village of Madhapur",
                ShortDescription = "Shilparamam is an arts and crafts village in Madhapur, Hyderabad, created as a place to showcase traditional crafts, culture, and artisan markets.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "craft", "culture", "market" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Shilparamam",
                QualityScore = 80
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd218"),
                PlaceId = DurgamCheruvuId,
                Title = "The Hidden Lake Near HITEC City",
                ShortDescription = "Durgam Cheruvu, also known as Raidurgam Cheruvu, is a freshwater lake near Hyderabad's HITEC City and Jubilee Hills area.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "nature", "lake", "cityscape" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Durgam_Cheruvu",
                QualityScore = 79
            },
            new StoryEntity
            {
                Id = Guid.Parse("98ce9e03-6434-4a95-a07d-f90b676fd219"),
                PlaceId = PaigahPalaceId,
                Title = "The Paigah Palace of Begumpet",
                ShortDescription = "Paigah Palace was built by Sir Vicar-ul-Umra, a Paigah nobleman, and later served civic and diplomatic functions in Hyderabad.",
                LanguageCode = "en",
                CategoriesJson = JsonSerializer.Serialize(new[] { "palace", "history", "architecture" }, JsonOptions),
                SourceName = "Wikipedia",
                SourceUrl = "https://en.wikipedia.org/wiki/Paigah_Palace",
                QualityScore = 78
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
