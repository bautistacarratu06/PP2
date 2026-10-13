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
    public async Task GetSectionRiskAsync_CalculatesRiskAndLevelPerStudent()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Ausente total", 0, 8, 0, 4),   // riesgo 100 -> Alto
            Row("Mixto", 16, 20, 5, 10),        // riesgo 32  -> Medio
            Row("Al día", 8, 8, 4, 4));         // riesgo 0   -> Bajo
        var service = new TrackingService(repo, new RiskConfig());

        var result = await service.GetSectionRiskAsync(1);

        Assert.Equal(1, repo.LastRequestedSection);
        Assert.Collection(result,
            r => { Assert.Equal(100, r.RiskScore); Assert.Equal(RiskLevel.Alto, r.RiskLevel); },
            r => { Assert.Equal(32, r.RiskScore); Assert.Equal(RiskLevel.Medio, r.RiskLevel); },
            r => { Assert.Equal(0, r.RiskScore); Assert.Equal(RiskLevel.Bajo, r.RiskLevel); });
    }

    [Fact]
    public async Task GetSectionRiskAsync_UsesConfiguredThresholds()
    {
        var repo = new FakeTrackingRepository().WithSection(1, Row("Mixto", 16, 20, 5, 10)); // riesgo 32
        var service = new TrackingService(repo, new RiskConfig { HighThreshold = 30, MediumThreshold = 10 });

        var result = await service.GetSectionRiskAsync(1);

        Assert.Equal(RiskLevel.Alto, Assert.Single(result).RiskLevel);
    }

    [Fact]
    public async Task GetSectionRiskAsync_EmptySection_ReturnsEmpty()
    {
        var service = new TrackingService(new FakeTrackingRepository(), new RiskConfig());

        var result = await service.GetSectionRiskAsync(99);

        Assert.Empty(result);
    }

    [Fact]
    public void Evaluate_InconsistentData_DoesNotThrow()
    {
        var service = new TrackingService(new FakeTrackingRepository(), new RiskConfig());

        // Más asistencias que clases (dato sucio): se recorta al total en vez de romper.
        var result = service.Evaluate(Row("Sucio", 10, 8, 0, 0));

        Assert.Equal(0, result.RiskScore);
        Assert.Equal(RiskLevel.Bajo, result.RiskLevel);
    }

    [Fact]
    public void Evaluate_NoClassesNorAssignments_ReturnsZeroRisk()
    {
        var service = new TrackingService(new FakeTrackingRepository(), new RiskConfig());

        var result = service.Evaluate(Row("Nuevo", 0, 0, 0, 0));

        Assert.Equal(0, result.RiskScore);
        Assert.Equal(100, result.Score);
    }
}
