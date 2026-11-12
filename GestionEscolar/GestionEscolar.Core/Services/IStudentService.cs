using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso StudentService para inyectarlo en la UI.
public interface IStudentService
{
    Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<Student> AddAsync(int sectionId, StudentDraft draft, DateTime today, CancellationToken cancellationToken = default);
    Task DeactivateAsync(IReadOnlyList<int> studentIds, CancellationToken cancellationToken = default);
}
