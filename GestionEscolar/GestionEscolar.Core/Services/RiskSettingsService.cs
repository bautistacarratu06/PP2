using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Lee y guarda los umbrales y los aplica al RiskConfig en memoria que usa la regla
// de seguimiento. Enteros de 0 a 100, y el medio estrictamente menor que el alto.
public class RiskSettingsService
{
    public const string NonNumericMessage = "Ingresá valores numéricos.";
    public const string InvalidThresholdMessage = "Los valores deben estar entre 0 y 100, y Medio debe ser menor que Alto.";
    public const string SavedMessage = "Umbrales guardados.";

    private readonly IRiskSettingsRepository _repository;
    private readonly RiskConfig _config;

    public RiskSettingsService(IRiskSettingsRepository repository, RiskConfig config)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(config);
        _repository = repository;
        _config = config;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var stored = await _repository.GetAsync(cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(stored);
        Apply(stored.HighThreshold, stored.MediumThreshold);
    }

    public async Task<RiskSettingsSaveResult> SaveAsync(
        string? highText,
        string? mediumText,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(highText, out var high) || !int.TryParse(mediumText, out var medium))
            return new RiskSettingsSaveResult(false, NonNumericMessage);

        if (high is < 0 or > 100 || medium is < 0 or > 100 || medium >= high)
            return new RiskSettingsSaveResult(false, InvalidThresholdMessage);

        await _repository.SaveAsync(high, medium, cancellationToken).ConfigureAwait(false);
        Apply(high, medium);
        return new RiskSettingsSaveResult(true, SavedMessage);
    }

    private void Apply(int high, int medium)
    {
        _config.HighThreshold = high;
        _config.MediumThreshold = medium;
    }
}

// Resultado de guardar: el mensaje ya está listo para mostrar en Ajustes.
public readonly record struct RiskSettingsSaveResult(bool WasSaved, string Message);
