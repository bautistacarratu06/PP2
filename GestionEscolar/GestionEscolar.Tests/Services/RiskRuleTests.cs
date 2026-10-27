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
    public void Given_ClassBeforeEnrollment_When_Counting_Then_ItStaysOutOfTheTotalAndTheAbsences()
    {
        var count = RiskRule.Count(
            new DateTime(2026, 4, 1),
            new DateTime(2026, 10, 27),
            [new RiskRule.DatedActivity(new DateTime(2026, 3, 15), false)]);

        Assert.Equal(0, count.Total);
        Assert.Equal(0, count.Completed);
    }

    [Fact]
    public void Given_FutureClassWithoutAMark_When_Counting_Then_NobodyIncludesIt()
    {
        var today = new DateTime(2026, 10, 27);
        var count = RiskRule.Count(
            new DateTime(2026, 4, 1),
            today,
            [new RiskRule.DatedActivity(today.AddDays(1), false)]);

        Assert.Equal(0, count.Total);
    }

    [Fact]
    public void Given_AssignmentDueAfterToday_When_Counting_Then_ItIsNotAMissingDelivery()
    {
        var today = new DateTime(2026, 10, 27);
        var count = RiskRule.Count(
            new DateTime(2026, 4, 1),
            today,
            [new RiskRule.DatedActivity(today.AddDays(2), false)]);

        Assert.Equal(0, count.Total);
        Assert.Equal(0, count.Completed);
    }

    [Fact]
    public void Given_ClassInsideTheWindowWithoutPresent_When_Counting_Then_ItIsAnAbsence()
    {
        var today = new DateTime(2026, 10, 27);
        var count = RiskRule.Count(
            new DateTime(2026, 4, 1),
            today,
            [new RiskRule.DatedActivity(today, false)]);

        Assert.Equal(1, count.Total);
        Assert.Equal(0, count.Completed);
    }

    [Fact]
    public void Given_NoClassOrAssignmentInsideTheWindow_When_Evaluating_Then_ThoseSignalsAreZero()
    {
        var today = new DateTime(2026, 10, 27);
        var enrolled = today;
        var classes = RiskRule.Count(enrolled, today, [new RiskRule.DatedActivity(today.AddDays(1), false)]);
        var assignments = RiskRule.Count(enrolled, today, [new RiskRule.DatedActivity(enrolled.AddDays(-3), false)]);

        var result = RiskRule.Evaluate(classes.Completed, classes.Total, assignments.Completed, assignments.Total, DefaultThresholds);

        Assert.Equal(0, classes.Total);
        Assert.Equal(0, assignments.Total);
        Assert.Equal(0, result.Score);
        Assert.Equal(RiskLevel.Bajo, result.Level);
    }

    [Fact]
    public void Given_SixteenOfTwentyAndFiveOfTenInsideTheWindow_When_Evaluating_Then_ScoreStays32()
    {
        var enrolled = new DateTime(2026, 1, 1);
        var today = new DateTime(2026, 10, 27);
        var classes = Enumerable.Range(0, 20)
            .Select(index => new RiskRule.DatedActivity(enrolled.AddDays(index + 1), index < 16))
            .Append(new RiskRule.DatedActivity(enrolled.AddDays(-1), false))
            .Append(new RiskRule.DatedActivity(today.AddDays(1), false));
        var assignments = Enumerable.Range(0, 10)
            .Select(index => new RiskRule.DatedActivity(enrolled.AddDays(index + 1), index < 5))
            .Append(new RiskRule.DatedActivity(today.AddDays(4), false));

        var classCount = RiskRule.Count(enrolled, today, classes);
        var assignmentCount = RiskRule.Count(enrolled, today, assignments);
        var result = RiskRule.Evaluate(
            classCount.Completed,
            classCount.Total,
            assignmentCount.Completed,
            assignmentCount.Total,
            DefaultThresholds);

        Assert.Equal(16, classCount.Completed);
        Assert.Equal(20, classCount.Total);
        Assert.Equal(5, assignmentCount.Completed);
        Assert.Equal(10, assignmentCount.Total);
        Assert.Equal(32, result.Score);
        Assert.Equal(RiskLevel.Medio, result.Level);
        Assert.Equal(RiskLevel.Alto, RiskRule.Classify(result.Score, new RiskConfig { HighThreshold = 30, MediumThreshold = 10 }));
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
