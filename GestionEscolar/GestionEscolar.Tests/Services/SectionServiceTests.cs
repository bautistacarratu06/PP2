using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class SectionServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsRepositorySections()
    {
        var service = new SectionService(new FakeSectionRepository(new Section(1, "Mañana - 1A"), new Section(2, "Tarde - 1B")));

        var sections = await service.GetAllAsync();

        Assert.Equal(new[] { 1, 2 }, sections.Select(s => s.Id));
    }

    [Fact]
    public void PickDefault_PrefersSectionOne()
    {
        var picked = SectionService.PickDefault([new Section(3, "C"), new Section(1, "A"), new Section(2, "B")]);

        Assert.Equal(1, picked?.Id);
    }

    [Fact]
    public void PickDefault_WithoutSectionOne_ReturnsFirst()
    {
        var picked = SectionService.PickDefault([new Section(4, "D"), new Section(2, "B")]);

        Assert.Equal(4, picked?.Id);
    }

    [Fact]
    public void PickDefault_Empty_ReturnsNull()
    {
        Assert.Null(SectionService.PickDefault([]));
    }

    [Fact]
    public async Task TrackingService_UsesRequestedSection()
    {
        var repo = new FakeTrackingRepository()
            .WithSection(1, new StudentTrackingData { Student = new Student { Name = "a" } })
            .WithSection(2,
                new StudentTrackingData { Student = new Student { Name = "b" } },
                new StudentTrackingData { Student = new Student { Name = "c" } });
        var service = new TrackingService(repo, new RiskConfig());

        var result = await service.GetSectionRiskAsync(2);

        Assert.Equal(2, repo.LastRequestedSection);
        Assert.Equal(2, result.Count);
    }
}
