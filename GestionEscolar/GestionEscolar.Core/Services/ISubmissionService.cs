using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso SubmissionService para inyectarlo en la UI.
public interface ISubmissionService
{
    Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubmissionMark>> GetMarksAsync(int assignmentId, CancellationToken cancellationToken = default);
    Task SaveAsync(int assignmentId, int studentId, SubmissionStatus status, CancellationToken cancellationToken = default);
    Task SaveManyAsync(int assignmentId, IReadOnlyList<int> studentIds, SubmissionStatus status, CancellationToken cancellationToken = default);
}
