using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace GestionEscolar.Tests.Services;

public class ScoringRiskServicesTests
{
    [Fact]
    public void Calculate_MixedCase_Returns32()
    {
        // Preparar: 20 clases y 10 trabajos en total
        var service = new ScoringRiskServices
        {
            TotalClasses = 20,
            TotalAssignments = 10
        };

        // Ejecutar: el alumno asistió a 16 clases y entregó 5 trabajos
        int risk = service.Calculate(16, 5);

        // Verificar: el riesgo tiene que ser 32
        Assert.Equal(32, risk);
    }

    [Theory]
    [InlineData(50, RiskLevel.Alto)]
    [InlineData(49, RiskLevel.Medio)]
    [InlineData(25, RiskLevel.Medio)]
    [InlineData(24, RiskLevel.Bajo)]
    public void GetRiskLevel_DefaultThresholds(int risk, RiskLevel expected)
    {
        var service = new ScoringRiskServices(new RiskConfig());

        Assert.Equal(expected, service.GetRiskLevel(risk));
    }

    [Fact]
    public void Calculate_MoreThanTotal_Throws()
    {
        var service = new ScoringRiskServices { TotalClasses = 8, TotalAssignments = 4 };

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Calculate(9, 0));
    }
}
