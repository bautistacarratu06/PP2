using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

public interface IAttendanceRepository
{
    // Un registro por alumno de la comisión de la clase (con o sin asistencia cargada).
    Task<IReadOnlyList<AttendanceMark>> GetMarksAsync(int classSessionId, CancellationToken cancellationToken = default);

    // Upsert por (clase, alumno).
    Task SaveAsync(int classSessionId, int studentId, AttendanceStatus status, CancellationToken cancellationToken = default);
}
