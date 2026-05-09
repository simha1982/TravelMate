using Microsoft.EntityFrameworkCore;
using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Tests;

public sealed class ContributionServiceTests
{
    [Fact]
    public async Task SubmitAsync_AddsContributionAndModerationResult()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfContributionRepository(dbContext);
        var service = new ContributionService(repository, new StubModelGateway());

        var contribution = await service.SubmitAsync(new SubmitContributionRequest(
            "contributor-1",
            "Nandi Hills",
            13.3702,
            77.6835,
            "en",
            "A local sunrise story",
            "This is a local pilot story about the sunrise, the road, and the hilltop experience."),
            CancellationToken.None);

        var queue = await service.GetQueueAsync(CancellationToken.None);
        var results = await service.GetModerationResultsAsync(contribution.Id, CancellationToken.None);

        Assert.Single(queue);
        Assert.Single(results);
        Assert.True(results.First().Passed);
    }

    private static TravelMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TravelMateDbContext(options);
    }
}
