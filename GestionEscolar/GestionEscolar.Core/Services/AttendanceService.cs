using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Registro de asistencia: clases de la comisión, asistencia por alumno y guardado.
public class AttendanceService
{
    private readonly ICalendarRepository _calendar;
    private readonly IAttendanceRepository _attendance;

    public AttendanceService(ICalendarRepository calendar, IAttendanceRepository attendance)
    {
        _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        _attendance = attendance ?? throw new ArgumentNullException(nameof(attendance));
    }

    public async Task<IReadOnlyList<ClassSession>> GetClassSessionsAsync(int sectionId, CancellationToken cancellationToken = default) =>
        (await _calendar.GetClassSessionsAsync(sectionId, cancellationToken).ConfigureAwait(false))
            .OrderBy(s => s.Date).ToList();

    public Task<IReadOnlyList<AttendanceMark>> GetMarksAsync(int classSessionId, CancellationToken cancellationToken = default) =>
        _attendance.GetMarksAsync(classSessionId, cancellationToken);

    public Task SaveAsync(int classSessionId, int studentId, AttendanceStatus status, CancellationToken cancellationToken = default) =>
        _attendance.SaveAsync(classSessionId, studentId, status, cancellationToken);

    // Por defecto: la última clase hasta hoy; si todas son futuras, la primera.
    public static ClassSession? PickDefault(IEnumerable<ClassSession> sessions, DateTime today)
    {
        var ordered = sessions.OrderBy(s => s.Date).ToList();
        return ordered.LastOrDefault(s => s.Date.Date <= today.Date) ?? ordered.FirstOrDefault();
    }
}
