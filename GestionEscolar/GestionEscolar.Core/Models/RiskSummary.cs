using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Cantidad de alumnos por nivel de riesgo en una comisión (tarjetas de Seguimiento).
public sealed record RiskSummary(int High, int Medium, int Low)
{
    public static RiskSummary Empty { get; } = new(0, 0, 0);

    public int Total => High + Medium + Low;

    // Cuenta los niveles de la lista ya evaluada. Las tarjetas de Seguimiento usan este conteo.
    public static RiskSummary FromResults(IEnumerable<StudentRiskResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        var list = results.ToList();
        return new RiskSummary(
            list.Count(r => r.RiskLevel == RiskLevel.Alto),
            list.Count(r => r.RiskLevel == RiskLevel.Medio),
            list.Count(r => r.RiskLevel == RiskLevel.Bajo));
    }
}
