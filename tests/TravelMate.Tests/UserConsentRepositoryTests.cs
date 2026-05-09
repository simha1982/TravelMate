using Microsoft.EntityFrameworkCore;
using TravelMate.Domain;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Tests;

public sealed class UserConsentRepositoryTests
{
    [Fact]
    public async Task GetAsync_ReturnsDeniedConsentByDefault()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfUserConsentRepository(dbContext);

        var consent = await repository.GetAsync("guest-user", CancellationToken.None);

        Assert.False(consent.LocationConsent);
        Assert.False(consent.VoiceConsent);
        Assert.False(consent.PersonalizationConsent);
    }

    [Fact]
    public async Task SaveAsync_StoresConsentChoices()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfUserConsentRepository(dbContext);
        var updatedAt = DateTimeOffset.UtcNow;

        await repository.SaveAsync(
            new UserConsent("guest-user", true, false, true, updatedAt),
            CancellationToken.None);

        var consent = await repository.GetAsync("guest-user", CancellationToken.None);

        Assert.True(consent.LocationConsent);
        Assert.False(consent.VoiceConsent);
        Assert.True(consent.PersonalizationConsent);
        Assert.Equal(updatedAt, consent.UpdatedAt);
    }

    private static TravelMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TravelMateDbContext(options);
    }
}
