using Microsoft.EntityFrameworkCore;

namespace TravelMate.Infrastructure.Persistence;

public sealed class TravelMateDbContext(DbContextOptions<TravelMateDbContext> options) : DbContext(options)
{
    public DbSet<PlaceEntity> Places => Set<PlaceEntity>();
    public DbSet<StoryEntity> Stories => Set<StoryEntity>();
    public DbSet<UserPreferenceEntity> UserPreferences => Set<UserPreferenceEntity>();
    public DbSet<PlaybackEventEntity> PlaybackEvents => Set<PlaybackEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlaceEntity>(entity =>
        {
            entity.HasKey(place => place.Id);
            entity.Property(place => place.Name).HasMaxLength(200).IsRequired();
            entity.Property(place => place.Country).HasMaxLength(100).IsRequired();
            entity.Property(place => place.Region).HasMaxLength(150).IsRequired();
            entity.Property(place => place.CategoriesJson).IsRequired();
            entity.HasIndex(place => new { place.Latitude, place.Longitude });
        });

        modelBuilder.Entity<StoryEntity>(entity =>
        {
            entity.HasKey(story => story.Id);
            entity.Property(story => story.Title).HasMaxLength(250).IsRequired();
            entity.Property(story => story.ShortDescription).HasMaxLength(1000).IsRequired();
            entity.Property(story => story.LanguageCode).HasMaxLength(10).IsRequired();
            entity.Property(story => story.CategoriesJson).IsRequired();
            entity.Property(story => story.SourceName).HasMaxLength(200).IsRequired();
            entity.Property(story => story.SourceUrl).HasMaxLength(1000).IsRequired();
            entity.Property(story => story.AudioUrl).HasMaxLength(1000);
            entity.HasIndex(story => new { story.PlaceId, story.LanguageCode });
        });

        modelBuilder.Entity<UserPreferenceEntity>(entity =>
        {
            entity.HasKey(preference => preference.UserId);
            entity.Property(preference => preference.UserId).HasMaxLength(100);
            entity.Property(preference => preference.InterestsJson).IsRequired();
            entity.Property(preference => preference.PreferredLanguageCode).HasMaxLength(10).IsRequired();
        });

        modelBuilder.Entity<PlaybackEventEntity>(entity =>
        {
            entity.HasKey(playbackEvent => playbackEvent.Id);
            entity.Property(playbackEvent => playbackEvent.UserId).HasMaxLength(100).IsRequired();
            entity.Property(playbackEvent => playbackEvent.Action).HasMaxLength(50).IsRequired();
            entity.HasIndex(playbackEvent => new { playbackEvent.UserId, playbackEvent.OccurredAt });
            entity.HasIndex(playbackEvent => playbackEvent.StoryId);
        });
    }
}

public sealed class PlaceEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string CategoriesJson { get; set; } = "[]";
}

public sealed class StoryEntity
{
    public Guid Id { get; set; }
    public Guid PlaceId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en";
    public string CategoriesJson { get; set; } = "[]";
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int QualityScore { get; set; }
}

public sealed class UserPreferenceEntity
{
    public string UserId { get; set; } = string.Empty;
    public string InterestsJson { get; set; } = "{}";
    public string PreferredLanguageCode { get; set; } = "en";
}

public sealed class PlaybackEventEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid StoryId { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}
