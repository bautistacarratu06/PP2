using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class CalendarServiceTests
{
    private static readonly DateTime D1 = new(2026, 9, 2);
    private static readonly DateTime D2 = new(2026, 9, 3);

    private static FakeCalendarRepository Repo()
    {
        var repo = new FakeCalendarRepository();
        repo.Sessions.Add(new ClassSession(1, 1, D2));
        repo.Sessions.Add(new ClassSession(2, 1, D1));
        repo.Sessions.Add(new ClassSession(3, 2, D1)); // otra comisión
        repo.Assignments.Add(new Assignment(10, 1, "Trabajo práctico 1", D2));
        return repo;
    }

    [Fact]
    public async Task GetItemsAsync_MergesAndSortsByDate_ClassBeforeAssignment()
    {
        var items = await new CalendarService(Repo()).GetItemsAsync(1);

        Assert.Collection(items,
            i => { Assert.Equal(CalendarItemKind.Clase, i.Kind); Assert.Equal(D1, i.Date); Assert.Null(i.Name); },
            i => { Assert.Equal(CalendarItemKind.Clase, i.Kind); Assert.Equal(D2, i.Date); },
            i => { Assert.Equal(CalendarItemKind.Trabajo, i.Kind); Assert.Equal("Trabajo práctico 1", i.Name); });
    }

    [Fact]
    public async Task AddClassAsync_DuplicateDateInSection_Throws()
    {
        var repo = Repo();
        var ex = await Assert.ThrowsAsync<CalendarValidationException>(
            () => new CalendarService(repo).AddClassAsync(1, D1.AddHours(10)));

        Assert.Contains("02/09/2026", ex.Message);
        Assert.Equal(3, repo.Sessions.Count);
    }

    [Fact]
    public async Task AddClassAsync_SameDateOtherSection_IsAllowed()
    {
        var repo = Repo();
        await new CalendarService(repo).AddClassAsync(2, D2);

        Assert.Contains(repo.Sessions, s => s.SectionId == 2 && s.Date == D2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddAssignmentAsync_NameRequired(string? name)
    {
        await Assert.ThrowsAsync<CalendarValidationException>(
            () => new CalendarService(Repo()).AddAssignmentAsync(1, name, D1));
    }

    [Fact]
    public async Task AddAssignmentAsync_TooLongName_Throws()
    {
        await Assert.ThrowsAsync<CalendarValidationException>(
            () => new CalendarService(Repo()).AddAssignmentAsync(1, new string('x', 121), D1));
    }

    [Fact]
    public async Task AddAssignmentAsync_TrimsNameAndDropsTime()
    {
        var repo = Repo();
        await new CalendarService(repo).AddAssignmentAsync(1, "  TP 2  ", D1.AddHours(15));

        var added = repo.Assignments.Last();
        Assert.Equal("TP 2", added.Name);
        Assert.Equal(D1, added.DueDate);
    }
}
