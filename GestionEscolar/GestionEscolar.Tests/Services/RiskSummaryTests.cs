using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Services;

public class RiskSummaryTests
{
    [Fact]
    public void Given_NoStudents_When_ReadingEmptySummary_Then_TotalIsZero()
    {
        Assert.Equal(0, RiskSummary.Empty.Total);
    }
}
