using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Arma el panel de seguimiento: trae los datos del repositorio y calcula el riesgo
// de cada alumno con ScoringRiskServices y los umbrales de RiskConfig.
public class TrackingService
{
    private readonly ITrackingRepository _repository;
    private readonly RiskConfig _config;

    public TrackingService(ITrackingRepository repository, RiskConfig config)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<IReadOnlyList<StudentRiskResult>> GetSectionRiskAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.GetRowsBySectionAsync(sectionId, cancellationToken).ConfigureAwait(false);
        return rows.Select(Evaluate).ToList();
    }

    public StudentRiskResult Evaluate(StudentTrackingData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Cada alumno puede tener totales distintos, así que se arma un scorer por fila.
        var scorer = new ScoringRiskServices(_config)
        {
            TotalClasses = data.TotalClasses,
            TotalAssignments = data.TotalAssignments
        };

        // Defensivo: si la base trae inconsistencias (más asistencias que clases) no rompemos la grilla.
        var attended = Math.Clamp(data.AttendedClasses, 0, data.TotalClasses);
        var submitted = Math.Clamp(data.SubmittedAssignments, 0, data.TotalAssignments);

        var risk = scorer.Calculate(attended, submitted);
        return new StudentRiskResult(data, risk, scorer.GetRiskLevel(risk));
    }
}
