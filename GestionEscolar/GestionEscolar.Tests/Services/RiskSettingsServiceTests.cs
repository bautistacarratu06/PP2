using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class RiskSettingsServiceTests
{
    [Fact]
    public async Task Given_Thresholds50And25InThePort_When_Loading_Then_TheRuleClassifiesWithThoseThresholds()
    {
        var port = new FakeRiskSettingsRepository(50, 25);
        var config = new RiskConfig { HighThreshold = 30, MediumThreshold = 10 };
        var service = new RiskSettingsService(port, config);
        var tracking = new TrackingService(
            new FakeTrackingRepository().WithSection(1, MixedRow()),
            config);

        await service.LoadAsync();

        Assert.Equal(50, config.HighThreshold);
        Assert.Equal(25, config.MediumThreshold);
        Assert.Equal(RiskLevel.Alto, RiskRule.Classify(50, config));
        Assert.Equal(RiskLevel.Medio, RiskRule.Classify(49, config));
        Assert.Equal(RiskLevel.Medio, RiskRule.Classify(25, config));
        Assert.Equal(RiskLevel.Bajo, RiskRule.Classify(24, config));

        var student = Assert.Single((await tracking.GetSectionRiskAsync(1)).Students);
        Assert.Equal(32, student.RiskScore);
        Assert.Equal(RiskLevel.Medio, student.RiskLevel);
    }

    [Fact]
    public async Task Given_ValidThresholds30And10_When_Saving_Then_PortKeepsThemAndRisk32IsHigh()
    {
        var port = new FakeRiskSettingsRepository(50, 25);
        var config = new RiskConfig();
        var service = new RiskSettingsService(port, config);
        var tracking = new TrackingService(
            new FakeTrackingRepository().WithSection(1, MixedRow()),
            config);

        var before = Assert.Single((await tracking.GetSectionRiskAsync(1)).Students);
        Assert.Equal(32, before.RiskScore);
        Assert.Equal(RiskLevel.Medio, before.RiskLevel);

        var result = await service.SaveAsync("30", "10");

        Assert.True(result.WasSaved);
        Assert.Equal(RiskSettingsService.SavedMessage, result.Message);
        Assert.Equal(1, port.SaveCount);
        Assert.Equal(30, port.HighThreshold);
        Assert.Equal(10, port.MediumThreshold);
        Assert.Equal(30, config.HighThreshold);
        Assert.Equal(10, config.MediumThreshold);

        var after = Assert.Single((await tracking.GetSectionRiskAsync(1)).Students);
        Assert.Equal(32, after.RiskScore);
        Assert.Equal(RiskLevel.Alto, after.RiskLevel);
    }

    [Fact]
    public async Task Given_High100AndMedium0_When_Saving_Then_PortKeepsThoseBounds()
    {
        var port = new FakeRiskSettingsRepository(50, 25);
        var config = new RiskConfig();
        var service = new RiskSettingsService(port, config);

        var result = await service.SaveAsync("100", "0");

        Assert.True(result.WasSaved);
        Assert.Equal(100, port.HighThreshold);
        Assert.Equal(0, port.MediumThreshold);
        Assert.Equal(100, config.HighThreshold);
        Assert.Equal(0, config.MediumThreshold);
    }

    [Theory]
    [InlineData("abc", "10")]
    [InlineData("30", "")]
    [InlineData("30", "10a")]
    [InlineData("30.5", "10")]
    [InlineData(null, "10")]
    public async Task Given_NonNumericThreshold_When_Saving_Then_SaveIsRejectedAndPortStaysUnchanged(string? high, string? medium)
    {
        await AssertRejected(high, medium, RiskSettingsService.NonNumericMessage);
    }

    [Theory]
    [InlineData("101", "10")]
    [InlineData("-1", "0")]
    [InlineData("50", "101")]
    [InlineData("50", "-1")]
    public async Task Given_ThresholdOutside0To100_When_Saving_Then_SaveIsRejectedAndPortStaysUnchanged(string? high, string? medium)
    {
        await AssertRejected(high, medium, RiskSettingsService.InvalidThresholdMessage);
    }

    [Theory]
    [InlineData("30", "30")]
    [InlineData("25", "40")]
    [InlineData("0", "0")]
    public async Task Given_MediumGreaterOrEqualToHigh_When_Saving_Then_SaveIsRejectedAndPortStaysUnchanged(string? high, string? medium)
    {
        await AssertRejected(high, medium, RiskSettingsService.InvalidThresholdMessage);
    }

    private static async Task AssertRejected(string? high, string? medium, string expectedMessage)
    {
        var port = new FakeRiskSettingsRepository(50, 25);
        var config = new RiskConfig();
        var service = new RiskSettingsService(port, config);

        var result = await service.SaveAsync(high, medium);

        Assert.False(result.WasSaved);
        Assert.Equal(expectedMessage, result.Message);
        Assert.Equal(0, port.SaveCount);
        Assert.Equal(50, port.HighThreshold);
        Assert.Equal(25, port.MediumThreshold);
        Assert.Equal(50, config.HighThreshold);
        Assert.Equal(25, config.MediumThreshold);
        Assert.Equal(RiskLevel.Medio, RiskRule.Classify(32, config));
    }

    // 16/20 asistencias y 5/10 entregas: riesgo 32.
    private static StudentTrackingData MixedRow() => new()
    {
        Student = new Student { Name = "Mixto", StudentNumber = "Mixto" },
        AttendedClasses = 16,
        TotalClasses = 20,
        SubmittedAssignments = 5,
        TotalAssignments = 10
    };
}
