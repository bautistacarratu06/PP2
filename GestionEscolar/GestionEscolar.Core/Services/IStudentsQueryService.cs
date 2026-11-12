using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso StudentsQueryService para inyectarlo en la UI.
public interface IStudentsQueryService
{
    Task<StudentsQueryResult> QueryAsync(int sectionId, StudentsQueryFilter? filter = null, CancellationToken cancellationToken = default);
}
