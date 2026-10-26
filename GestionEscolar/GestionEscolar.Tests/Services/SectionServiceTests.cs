using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class SectionServiceTests
{
    [Fact]
    public async Task Given_RepositoryWithSections_When_Listing_Then_ReturnsThoseSections()
    {
        var service = new SectionService(new FakeSectionRepository(new Section(1, "Mañana - 1A"), new Section(2, "Tarde - 1B")));

        var sections = await service.GetAllAsync();

        Assert.Equal(new[] { 1, 2 }, sections.Select(s => s.Id));
    }

    [Fact]
    public void Given_ListIncludingSectionOne_When_PickingDefault_Then_IdIsOne()
    {
        var picked = SectionService.PickDefault([new Section(3, "C"), new Section(1, "A"), new Section(2, "B")]);

        Assert.Equal(1, picked?.Id);
    }

    [Fact]
    public void Given_ListWithoutSectionOne_When_PickingDefault_Then_FirstIsChosen()
    {
        var picked = SectionService.PickDefault([new Section(4, "D"), new Section(2, "B")]);

        Assert.Equal(4, picked?.Id);
    }

    [Fact]
    public void Given_EmptyList_When_PickingDefault_Then_ResultIsEmpty()
    {
        Assert.Null(SectionService.PickDefault([]));
    }

    [Fact]
    public void Given_SectionWithName_When_ReadingText_Then_TextIsTheName()
    {
        var section = new Section(1, "Mañana - 1A");

        Assert.Equal("Mañana - 1A", section.ToString());
    }
}
