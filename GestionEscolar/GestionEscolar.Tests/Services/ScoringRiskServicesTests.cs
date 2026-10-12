/*using UI.Services;

namespace GestionEscolar.Tests.Services;

public class ScoringRiskServicesTests
{
    [Fact]
    public void Calculate_MixedCase_Returns32()
    {
        // Preparar: 20 clases y 10 trabajos en total
        var service = new ScoringRiskServices(new RiskConfigServices())
        {
            TotalClasses = 20,
            TotalAssignments = 10
        };

        // Ejecutar: el alumno asistió a 16 clases y entregó 5 trabajos
        int risk = service.Calculate(16, 5);

        // Verificar: el riesgo tiene que ser 32
        Assert.Equal(32, risk);
    }
}
*/