using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

public interface ISubmissionRepository
{
    // Un registro por alumno de la comisión del trabajo (con o sin entrega cargada).
    Task<IReadOnlyList<SubmissionMark>> GetMarksAsync(int assignmentId, CancellationToken cancellationToken = default);

    // Upsert por (trabajo, alumno).
    Task SaveAsync(int assignmentId, int studentId, SubmissionStatus status, CancellationToken cancellationToken = default);
}
