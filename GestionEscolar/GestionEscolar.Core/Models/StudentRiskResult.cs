using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Resultado de TrackingService: los datos de seguimiento más el riesgo calculado.
public sealed record StudentRiskResult(StudentTrackingData Data, int RiskScore, RiskLevel RiskLevel)
{
    // Score "positivo" (100 - riesgo), para mostrar en la justificación.
    public int Score => Math.Clamp(100 - RiskScore, 0, 100);
}
