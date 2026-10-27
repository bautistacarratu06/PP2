using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Resumen de riesgo por comisión, con la misma regla que RiskRule.
// La pantalla de seguimiento no lo usa: cuenta los niveles de la lista ya evaluada.
public interface IRiskSummaryRepository
{
    Task<RiskSummary> GetSummaryBySectionAsync(int sectionId, CancellationToken cancellationToken = default);
}
