using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Registro de entregas: trabajos de la comisión, entrega por alumno y guardado.
public class SubmissionService
{
    private readonly ICalendarRepository _calendar;
    private readonly ISubmissionRepository _submissions;

    public SubmissionService(ICalendarRepository calendar, ISubmissionRepository submissions)
    {
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _submissions = submissions ?? throw new ArgumentNullException(nameof(submissions));
    }

    public async Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(int sectionId, CancellationToken cancellationToken = default) =>
        (await _calendar.GetAssignmentsAsync(sectionId, cancellationToken).ConfigureAwait(false))
            .OrderBy(a => a.DueDate).ThenBy(a => a.Name, StringComparer.CurrentCulture).ToList();

    public Task<IReadOnlyList<SubmissionMark>> GetMarksAsync(int assignmentId, CancellationToken cancellationToken = default) =>
        _submissions.GetMarksAsync(assignmentId, cancellationToken);

    public Task SaveAsync(int assignmentId, int studentId, SubmissionStatus status, CancellationToken cancellationToken = default) =>
        _submissions.SaveAsync(assignmentId, studentId, status, cancellationToken);

    // Por defecto: el último trabajo vencido hasta hoy; si todos son futuros, el primero.
    public static Assignment? PickDefault(IEnumerable<Assignment> assignments, DateTime today)
    {
        var ordered = assignments.OrderBy(a => a.DueDate).ToList();
        return ordered.LastOrDefault(a => a.DueDate.Date <= today.Date) ?? ordered.FirstOrDefault();
    }
}
