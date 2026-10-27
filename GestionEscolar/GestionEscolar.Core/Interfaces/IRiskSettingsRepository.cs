using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Puerto de los dos umbrales de riesgo persistidos en dbo.setting.
public interface IRiskSettingsRepository
{
    Task<RiskConfig> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(int highThreshold, int mediumThreshold, CancellationToken cancellationToken = default);
}
