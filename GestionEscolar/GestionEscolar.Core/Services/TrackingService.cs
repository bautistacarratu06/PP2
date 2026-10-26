using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Caso de uso de seguimiento: evalúa a cada alumno con la regla de riesgo y devuelve
// esa lista junto con el resumen Alto/Medio/Bajo contado sobre la misma lista.
// Umbrales por defecto: 50 y 25.
public class TrackingService
{
    private readonly ITrackingRepository _repository;
    private readonly RiskConfig _config;

    public TrackingService(ITrackingRepository repository, RiskConfig config)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<SectionTracking> GetSectionRiskAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.GetRowsBySectionAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var students = rows.Select(Evaluate).ToList();
        return new SectionTracking(students, RiskSummary.FromResults(students));
    }

    private StudentRiskResult Evaluate(StudentTrackingData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Dato sucio (más asistencias que clases): se recorta al total. Un total negativo
        // no se recorta acá: lo rechaza la regla.
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
