using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso AttendanceService para inyectarlo en la UI.
public interface IAttendanceService
{
    Task<IReadOnlyList<ClassSession>> GetClassSessionsAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceMark>> GetMarksAsync(int classSessionId, CancellationToken cancellationToken = default);
    Task SaveAsync(int classSessionId, int studentId, AttendanceStatus status, CancellationToken cancellationToken = default);
    Task SaveManyAsync(int classSessionId, IReadOnlyList<int> studentIds, AttendanceStatus status, CancellationToken cancellationToken = default);
}
