using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace GestionEscolar.Data.Repositories;

// Cuenta niveles con RiskRule sobre las mismas filas crudas del seguimiento.
// La pantalla no lo usa: las tarjetas salen de la lista que ya evaluó TrackingService.
public sealed class EfRiskSummaryRepository(ITrackingRepository tracking, RiskConfig config) : IRiskSummaryRepository
{
    private readonly ITrackingRepository _tracking = tracking ?? throw new ArgumentNullException(nameof(tracking));
    private readonly RiskConfig _config = config ?? throw new ArgumentNullException(nameof(config));

    public async Task<RiskSummary> GetSummaryBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = await _tracking.GetRowsBySectionAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var results = rows.Select(Evaluate);
        return RiskSummary.FromResults(results);
    }

    private StudentRiskResult Evaluate(StudentTrackingData data)
    {
        var attended = data.TotalClasses < 0
            ? data.AttendedClasses
            : Math.Clamp(data.AttendedClasses, 0, data.TotalClasses);
        var submitted = data.TotalAssignments < 0
            ? data.SubmittedAssignments
            : Math.Clamp(data.SubmittedAssignments, 0, data.TotalAssignments);

        var evaluation = RiskRule.Evaluate(
            attended,
            data.TotalClasses,
            submitted,
            data.TotalAssignments,
            _config);
        return new StudentRiskResult(data, evaluation.Score, evaluation.Level);
    }
}
