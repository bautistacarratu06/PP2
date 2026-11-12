using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso TrackingService para inyectarlo en la UI.
public interface ITrackingService
{
    Task<SectionTracking> GetSectionRiskAsync(int sectionId, CancellationToken cancellationToken = default);
}
