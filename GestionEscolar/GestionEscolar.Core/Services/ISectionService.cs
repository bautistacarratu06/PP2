using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso SectionService para inyectarlo en la UI.
public interface ISectionService
{
    Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default);
}
