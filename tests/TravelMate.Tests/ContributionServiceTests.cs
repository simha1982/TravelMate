using Microsoft.EntityFrameworkCore;
using TravelMate.AI;
using TravelMate.Application;
using TravelMate.Domain;
using TravelMate.Infrastructure.Persistence;

namespace TravelMate.Tests;

public sealed class ContributionServiceTests
{
    [Fact]
    public async Task SubmitAsync_AddsContributionAndModerationResult()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfContributionRepository(dbContext);
        var travelMateRepository = new EfTravelMateRepository(dbContext);
        var service = new ContributionService(repository, travelMateRepository, new StubModelGateway());

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

    [Fact]
    public async Task ApproveAndPublishAsync_CreatesSearchableStory()
    {
        await using var dbContext = CreateDbContext();
        var contributionRepository = new EfContributionRepository(dbContext);
        var travelMateRepository = new EfTravelMateRepository(dbContext);
        var service = new ContributionService(contributionRepository, travelMateRepository, new StubModelGateway());

        var contribution = await service.SubmitAsync(new SubmitContributionRequest(
            "contributor-1",
            "Hyderabad",
            17.3616,
            78.4747,
            "en",
            "A lane near Charminar",
            "The market lanes around Charminar carry the sound of bangles, tea, and old Hyderabad stories."),
            CancellationToken.None);

        var published = await service.ApproveAndPublishAsync(contribution.Id, CancellationToken.None);
        var stories = await travelMateRepository.GetStoriesAsync(CancellationToken.None);

        Assert.NotNull(published);
        Assert.Equal(ContributionStatus.Approved, published.Contribution.Status);
        Assert.Contains(stories, story => story.Id == published.Story.Id && story.Title == contribution.Title);
    }

    private static TravelMateDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TravelMateDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new TravelMateDbContext(options);
    }
}
