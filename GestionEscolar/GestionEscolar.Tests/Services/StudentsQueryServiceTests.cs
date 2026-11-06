using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class StudentsQueryServiceTests
{
    private static StudentTrackingData Row(
        string last,
        string first,
        string number,
        int attended,
        int totalClasses,
        int submitted,
        int totalAssignments,
        bool active = true,
        DateTime? lastContact = null,
        string email = "") =>
        new()
        {
            Student = new Student
            {
                LastName = last,
                FirstName = first,
                Name = first + " " + last,
                StudentNumber = number,
                Email = email,
                IsActive = active
            },
            AttendedClasses = attended,
            TotalClasses = totalClasses,
            SubmittedAssignments = submitted,
            TotalAssignments = totalAssignments,
            LastContact = lastContact
        };

    [Fact]
    public async Task Given_ActiveAndInactive_When_QueryingDefault_Then_OnlyActivesAndSummaryIgnoresInactive()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Alto", "A", "1", 0, 8, 0, 4, active: true),
            Row("Bajo", "B", "2", 8, 8, 4, 4, active: true),
            Row("Inactivo", "C", "3", 0, 8, 0, 4, active: false));
        var service = new StudentsQueryService(repo, new RiskConfig());

        var result = await service.QueryAsync(1);

        Assert.Equal(2, result.Students.Count);
        Assert.All(result.Students, s => Assert.True(s.Data.Student.IsActive));
        Assert.Equal(new RiskSummary(1, 0, 1), result.ActiveRiskSummary);
    }

    [Fact]
    public async Task Given_RiskFilter_When_Querying_Then_OnlyMatchingLevels()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("A", "Uno", "1", 0, 8, 0, 4),
            Row("B", "Dos", "2", 16, 20, 5, 10),
            Row("C", "Tres", "3", 8, 8, 4, 4));
        var service = new StudentsQueryService(repo, new RiskConfig());

        var result = await service.QueryAsync(1, new StudentsQueryFilter
        {
            RiskLevels = [RiskLevel.Alto, RiskLevel.Bajo]
        });

        Assert.Equal(2, result.Students.Count);
        Assert.DoesNotContain(result.Students, s => s.RiskLevel == RiskLevel.Medio);
    }

    [Fact]
    public async Task Given_AttendanceRangeAndName_When_Querying_Then_AndCombinesFilters()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("García", "Ana", "10", 8, 10, 4, 4),
            Row("García", "Luis", "11", 2, 10, 4, 4),
            Row("Pérez", "Ana", "12", 9, 10, 4, 4));
        var service = new StudentsQueryService(repo, new RiskConfig());

        var result = await service.QueryAsync(1, new StudentsQueryFilter
        {
            NameContains = "García",
            AttendanceMinPercent = 50
        });

        var only = Assert.Single(result.Students);
        Assert.Equal("Ana", only.Data.Student.FirstName);
        Assert.Equal(80, only.Data.AttendancePercent);
    }

    [Fact]
    public async Task Given_DefaultSort_When_Querying_Then_OrderedByLastThenFirstName()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Pérez", "Zoe", "1", 8, 8, 4, 4),
            Row("Álvarez", "Ana", "2", 8, 8, 4, 4),
            Row("Pérez", "Ana", "3", 8, 8, 4, 4));
        var service = new StudentsQueryService(repo, new RiskConfig());

        var result = await service.QueryAsync(1);

        Assert.Equal(["Álvarez", "Pérez", "Pérez"], result.Students.Select(s => s.Data.Student.LastName));
        Assert.Equal("Ana", result.Students[1].Data.Student.FirstName);
        Assert.Equal("Zoe", result.Students[2].Data.Student.FirstName);
    }

    [Fact]
    public async Task Given_InactiveFilter_When_Querying_Then_OnlyInactives()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("A", "Uno", "1", 8, 8, 4, 4, active: true),
            Row("B", "Dos", "2", 8, 8, 4, 4, active: false));
        var service = new StudentsQueryService(repo, new RiskConfig());

        var result = await service.QueryAsync(1, new StudentsQueryFilter
        {
            ActiveStatus = ActiveStatusFilter.InactiveOnly
        });

        var only = Assert.Single(result.Students);
        Assert.False(only.Data.Student.IsActive);
        Assert.Equal(new RiskSummary(0, 0, 1), result.ActiveRiskSummary);
    }

    [Fact]
    public async Task Given_ContactFilter_When_Querying_Then_MatchesContacted()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("A", "Uno", "1", 8, 8, 4, 4, lastContact: null),
            Row("B", "Dos", "2", 8, 8, 4, 4, lastContact: new DateTime(2026, 1, 1)));
        var service = new StudentsQueryService(repo, new RiskConfig());

        var result = await service.QueryAsync(1, new StudentsQueryFilter
        {
            ContactStatuses = [ContactStatusFilter.SinContactar]
        });

        Assert.Equal("1", Assert.Single(result.Students).Data.Student.StudentNumber);
    }
}
