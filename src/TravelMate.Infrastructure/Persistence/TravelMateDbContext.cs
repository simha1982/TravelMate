using Microsoft.EntityFrameworkCore;

namespace TravelMate.Infrastructure.Persistence;

public sealed class TravelMateDbContext(DbContextOptions<TravelMateDbContext> options) : DbContext(options)
{
    public DbSet<PlaceEntity> Places => Set<PlaceEntity>();
    public DbSet<StoryEntity> Stories => Set<StoryEntity>();
    public DbSet<UserPreferenceEntity> UserPreferences => Set<UserPreferenceEntity>();
    public DbSet<PlaybackEventEntity> PlaybackEvents => Set<PlaybackEventEntity>();
    public DbSet<ContributionEntity> Contributions => Set<ContributionEntity>();
    public DbSet<ModerationResultEntity> ModerationResults => Set<ModerationResultEntity>();

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

        modelBuilder.Entity<ContributionEntity>(entity =>
        {
            entity.HasKey(contribution => contribution.Id);
            entity.Property(contribution => contribution.ContributorUserId).HasMaxLength(100).IsRequired();
            entity.Property(contribution => contribution.PlaceName).HasMaxLength(200).IsRequired();
            entity.Property(contribution => contribution.LanguageCode).HasMaxLength(10).IsRequired();
            entity.Property(contribution => contribution.Title).HasMaxLength(250).IsRequired();
            entity.Property(contribution => contribution.StoryText).HasMaxLength(8000).IsRequired();
            entity.Property(contribution => contribution.Status).HasMaxLength(50).IsRequired();
            entity.HasIndex(contribution => new { contribution.Status, contribution.SubmittedAt });
        });

        modelBuilder.Entity<ModerationResultEntity>(entity =>
        {
            entity.HasKey(result => result.Id);
            entity.Property(result => result.Summary).HasMaxLength(2000).IsRequired();
            entity.Property(result => result.FlagsJson).IsRequired();
            entity.HasIndex(result => result.ContributionId);
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

public sealed class ContributionEntity
{
    public Guid Id { get; set; }
    public string ContributorUserId { get; set; } = string.Empty;
    public string PlaceName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string LanguageCode { get; set; } = "en";
    public string Title { get; set; } = string.Empty;
    public string StoryText { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted";
    public DateTimeOffset SubmittedAt { get; set; }
}

public sealed class ModerationResultEntity
{
    public Guid Id { get; set; }
    public Guid ContributionId { get; set; }
    public bool Passed { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string FlagsJson { get; set; } = "[]";
    public DateTimeOffset ReviewedAt { get; set; }
}
