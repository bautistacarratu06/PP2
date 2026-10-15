using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Resumen de riesgo por comisión (Alto / Medio / Bajo).
// La implementación debe usar la misma fórmula y umbrales que ScoringRiskServices + RiskConfig.
public interface IRiskSummaryRepository
{
    Task<RiskSummary> GetSummaryBySectionAsync(int sectionId, CancellationToken cancellationToken = default);
}
