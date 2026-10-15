using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Acceso a los datos de seguimiento. La implementación SQL vive en GestionEscolar.Data;
// en los tests se usa un repositorio falso.
public interface IStudentRepository
{
    Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default);
}
