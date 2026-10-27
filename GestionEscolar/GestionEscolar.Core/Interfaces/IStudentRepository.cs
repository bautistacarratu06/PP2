using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Nómina de una comisión. La implementación vive en GestionEscolar.Data;
// en los tests se usa un repositorio falso.
public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default);
}
