using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Filas crudas de seguimiento. La implementación vive en GestionEscolar.Data;
// en los tests se usa un repositorio falso. El riesgo lo calcula TrackingService.
public interface ITrackingRepository
{
    Task<IReadOnlyList<StudentTrackingData>> GetRowsBySectionAsync(int sectionId, CancellationToken cancellationToken = default);
}
