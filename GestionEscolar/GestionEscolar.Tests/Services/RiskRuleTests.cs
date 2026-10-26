using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace GestionEscolar.Tests.Services;

public class RiskRuleTests
{
    private static readonly RiskConfig DefaultThresholds = new();

    [Fact]
    public void Given_MixedAttendanceAndDeliveries_When_Calculating_Then_ScoreIs32()
    {
        var result = RiskRule.Evaluate(16, 20, 5, 10, DefaultThresholds);

        Assert.Equal(32, result.Score);
        Assert.Equal(RiskLevel.Medio, result.Level);
    }

    [Theory]
    [InlineData(50, RiskLevel.Alto)]
    [InlineData(49, RiskLevel.Medio)]
    [InlineData(25, RiskLevel.Medio)]
    [InlineData(24, RiskLevel.Bajo)]
    public void Given_Thresholds50And25_When_ClassifyingScore_Then_LevelMatches(int score, RiskLevel expected)
    {
        Assert.Equal(expected, RiskRule.Classify(score, DefaultThresholds));
    }

    [Fact]
    public void Given_FactGreaterThanTotal_When_Calculating_Then_Rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RiskRule.Evaluate(9, 8, 0, 4, DefaultThresholds));
    }

    [Fact]
    public void Given_NegativeTotal_When_Calculating_Then_Rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RiskRule.Evaluate(0, -1, 0, 4, DefaultThresholds));
    }

    [Fact]
    public void Given_ZeroClassesAndAssignments_When_Calculating_Then_RiskIsZero()
    {
        var result = RiskRule.Evaluate(0, 0, 0, 0, DefaultThresholds);

        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Given_HalfPointRisk_When_Calculating_Then_RoundsAwayFromZero()
    {
        // 0.125 * 0.6 + 0.25 * 0.4 = 0.175 → 17.5 → 18 (AwayFromZero).
        var result = RiskRule.Evaluate(7, 8, 3, 4, DefaultThresholds);

        Assert.Equal(18, result.Score);
    }

    [Theory]
    [InlineData(0, 8, 0, 4, 100)]
    [InlineData(4, 8, 2, 4, 50)]
    [InlineData(6, 8, 3, 4, 25)]
    [InlineData(7, 8, 3, 4, 18)]
    [InlineData(0, 0, 0, 0, 0)]
    public void Given_AttendanceAndDeliveries_When_Calculating_Then_ScoreMatchesRule(
        int attended, int totalClasses, int submitted, int totalAssignments, int expected)
    {
        var result = RiskRule.Evaluate(attended, totalClasses, submitted, totalAssignments, DefaultThresholds);

        Assert.Equal(expected, result.Score);
    }

    [Fact]
    public void Given_AttendedAndTotals_When_ReadingPercents_Then_MatchGridAndZeroTotalIsZero()
    {
        var mixed = new StudentTrackingData
        {
            AttendedClasses = 16,
            TotalClasses = 20,
            SubmittedAssignments = 5,
            TotalAssignments = 10
        };
        var none = new StudentTrackingData();

        Assert.Equal(80, mixed.AttendancePercent);
        Assert.Equal(50, mixed.DeliveryPercent);
        Assert.Equal(0, none.AttendancePercent);
        Assert.Equal(0, none.DeliveryPercent);
    }
}
