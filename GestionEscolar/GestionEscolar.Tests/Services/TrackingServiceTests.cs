using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class TrackingServiceTests
{
    private static StudentTrackingData Row(string name, int attended, int totalClasses, int submitted, int totalAssignments) =>
        new()
        {
            Student = new Student { Name = name, StudentNumber = name },
            AttendedClasses = attended,
            TotalClasses = totalClasses,
            SubmittedAssignments = submitted,
            TotalAssignments = totalAssignments
        };

    [Fact]
    public async Task Given_SectionStudents_When_RequestingTracking_Then_EachGetsScoreAndLevel()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Ausente total", 0, 8, 0, 4),   // riesgo 100 -> Alto
            Row("Mixto", 16, 20, 5, 10),        // riesgo 32  -> Medio
            Row("Al día", 8, 8, 4, 4));         // riesgo 0   -> Bajo
        var service = new TrackingService(repo, new RiskConfig());

        var result = await service.GetSectionRiskAsync(1);

        Assert.Equal(1, repo.LastRequestedSection);
        Assert.Collection(result.Students,
            r => { Assert.Equal(100, r.RiskScore); Assert.Equal(RiskLevel.Alto, r.RiskLevel); },
            r => { Assert.Equal(32, r.RiskScore); Assert.Equal(RiskLevel.Medio, r.RiskLevel); },
            r => { Assert.Equal(0, r.RiskScore); Assert.Equal(RiskLevel.Bajo, r.RiskLevel); });
    }

    [Fact]
    public async Task Given_HighMediumAndLowStudents_When_RequestingTracking_Then_SummaryCountsThoseLevels()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("a", 0, 8, 0, 4),     // 100 -> Alto
            Row("b", 4, 8, 2, 4),     // 50  -> Alto (borde)
            Row("c", 16, 20, 5, 10),  // 32  -> Medio
            Row("d", 6, 8, 3, 4),     // 25  -> Medio (borde)
            Row("e", 8, 8, 3, 4),     // 10  -> Bajo
            Row("f", 8, 8, 4, 4));    // 0   -> Bajo
        var result = await new TrackingService(repo, new RiskConfig()).GetSectionRiskAsync(1);

        Assert.Equal(new RiskSummary(2, 2, 2), result.Summary);
        Assert.Equal(result.Students.Count, result.Summary.Total);
        Assert.Equal(RiskSummary.FromResults(result.Students), result.Summary);
    }

    [Fact]
    public async Task Given_ConfiguredThresholds_When_Evaluating_Then_LevelUsesThoseThresholds()
    {
        var repo = new FakeTrackingRepository().WithSection(1, Row("Mixto", 16, 20, 5, 10)); // riesgo 32
        var service = new TrackingService(repo, new RiskConfig { HighThreshold = 30, MediumThreshold = 10 });

        var result = await service.GetSectionRiskAsync(1);

        Assert.Equal(RiskLevel.Alto, Assert.Single(result.Students).RiskLevel);
    }

    [Fact]
    public async Task Given_EmptySection_When_RequestingTracking_Then_ListIsEmptyAndSummaryIsZero()
    {
        var service = new TrackingService(new FakeTrackingRepository(), new RiskConfig());

        var result = await service.GetSectionRiskAsync(99);

        Assert.Empty(result.Students);
        Assert.Equal(RiskSummary.Empty, result.Summary);
        Assert.Equal(0, result.Summary.Total);
    }

    [Fact]
    public async Task Given_MoreAttendancesThanClasses_When_Evaluating_Then_DoesNotThrowAndUsesClampedTotal()
    {
        var repo = new FakeTrackingRepository().WithSection(1, Row("Sucio", 10, 8, 0, 0));
        var service = new TrackingService(repo, new RiskConfig());

        var result = await service.GetSectionRiskAsync(1);

        var student = Assert.Single(result.Students);
        Assert.Equal(0, student.RiskScore);
        Assert.Equal(RiskLevel.Bajo, student.RiskLevel);
    }

    [Fact]
    public async Task Given_NoClassesNorAssignments_When_Evaluating_Then_RiskIsZero()
    {
        var repo = new FakeTrackingRepository().WithSection(1, Row("Nuevo", 0, 0, 0, 0));
        var service = new TrackingService(repo, new RiskConfig());

        var result = await service.GetSectionRiskAsync(1);

        var student = Assert.Single(result.Students);
        Assert.Equal(0, student.RiskScore);
        Assert.Equal(100, student.Score);
    }

    [Fact]
    public async Task Given_RequestedSectionId_When_Querying_Then_RowRepositoryReceivesThatId()
    {
        var repo = new FakeTrackingRepository()
            .WithSection(1, new StudentTrackingData { Student = new Student { Name = "a" } })
            .WithSection(2,
                new StudentTrackingData { Student = new Student { Name = "b" } },
                new StudentTrackingData { Student = new Student { Name = "c" } });
        var service = new TrackingService(repo, new RiskConfig());

        var result = await service.GetSectionRiskAsync(2);

        Assert.Equal(2, repo.LastRequestedSection);
        Assert.Equal(2, result.Students.Count);
    }
}
