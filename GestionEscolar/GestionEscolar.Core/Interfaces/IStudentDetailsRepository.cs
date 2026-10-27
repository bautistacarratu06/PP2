using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Datos del detalle de un alumno. studentId es dbo.student.id.
public interface IStudentDetailsRepository
{
    // Datos de contacto + comisión. null si el alumno no existe.
    Task<StudentDetail?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default);

    // Una fila por clase de la comisión (con o sin registro de asistencia), más reciente primero.
    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(int studentId, int sectionId, CancellationToken cancellationToken = default);

    // Una fila por trabajo de la comisión (con o sin registro de entrega), por fecha de vencimiento.
    Task<IReadOnlyList<Submission>> GetSubmissionsAsync(int studentId, int sectionId, CancellationToken cancellationToken = default);

    // Intervenciones del alumno, más reciente primero.
    Task<IReadOnlyList<Intervention>> GetInterventionsAsync(int studentId, CancellationToken cancellationToken = default);

    Task<Intervention> AddInterventionAsync(Intervention intervention, CancellationToken cancellationToken = default);
}
