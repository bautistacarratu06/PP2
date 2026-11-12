using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso RiskSettingsService para inyectarlo en la UI.
public interface IRiskSettingsService
{
    Task LoadAsync(CancellationToken cancellationToken = default);
    Task<RiskSettingsSaveResult> SaveAsync(string? highText, string? mediumText, CancellationToken cancellationToken = default);
}
