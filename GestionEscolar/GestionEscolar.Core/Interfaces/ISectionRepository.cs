using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

public interface ISectionRepository
{
    Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default);
}
