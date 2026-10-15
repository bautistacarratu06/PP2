using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class RiskSummaryTests
{
    private static StudentTrackingData Row(int attended, int totalClasses, int submitted, int totalAssignments) =>
        new()
        {
            Student = new Student { Name = "x" },
            AttendedClasses = attended,
            TotalClasses = totalClasses,
            SubmittedAssignments = submitted,
            TotalAssignments = totalAssignments
        };

    [Fact]
    public async Task FromResults_CountsMatchLevelsShownInTable()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row(0, 8, 0, 4),     // 100 -> Alto
            Row(4, 8, 2, 4),     // 50  -> Alto (borde)
            Row(16, 20, 5, 10),  // 32  -> Medio
            Row(6, 8, 3, 4),     // 25  -> Medio (borde)
            Row(8, 8, 3, 4),     // 10  -> Bajo
            Row(8, 8, 4, 4));    // 0   -> Bajo
        var results = await new TrackingService(repo, new RiskConfig()).GetSectionRiskAsync(1);

        var summary = RiskSummary.FromResults(results);

        Assert.Equal(new RiskSummary(2, 2, 2), summary);
        Assert.Equal(results.Count, summary.Total);
    }

    [Fact]
    public void Empty_HasZeroTotal()
    {
        Assert.Equal(0, RiskSummary.Empty.Total);
    }

    // Documenta la fórmula que replica SqlRiskSummaryRepository: si este test cambia,
    // hay que actualizar la query SQL.
    [Theory]
    [InlineData(0, 8, 0, 4, 100)]
    [InlineData(4, 8, 2, 4, 50)]
    [InlineData(6, 8, 3, 4, 25)]
    [InlineData(7, 8, 3, 4, 18)] // 0.125*0.6 + 0.25*0.4 = 0.175 -> 17.5 -> 18 (AwayFromZero)
    [InlineData(0, 0, 0, 0, 0)]
    public void Formula_MatchesSqlEquivalent(int attended, int totalClasses, int submitted, int totalAssignments, int expected)
    {
        var scorer = new ScoringRiskServices { TotalClasses = totalClasses, TotalAssignments = totalAssignments };
        var absence = totalClasses == 0 ? 0.0 : (double)(totalClasses - attended) / totalClasses;
        var missing = totalAssignments == 0 ? 0.0 : (double)(totalAssignments - submitted) / totalAssignments;
        var sqlEquivalent = (int)Math.Round((absence * 0.6 + missing * 0.4) * 100, MidpointRounding.AwayFromZero);

        Assert.Equal(expected, scorer.Calculate(attended, submitted));
        Assert.Equal(expected, sqlEquivalent);
    }
}
