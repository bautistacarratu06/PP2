using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Nómina de una comisión. La implementación vive en GestionEscolar.Data;
// en los tests se usa un repositorio falso.
public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default);

    Task<Student?> FindByDniAsync(string dni, CancellationToken cancellationToken = default);

    Task<Student?> FindByStudentNumberAsync(int sectionId, string studentNumber, CancellationToken cancellationToken = default);

    Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default);
}
