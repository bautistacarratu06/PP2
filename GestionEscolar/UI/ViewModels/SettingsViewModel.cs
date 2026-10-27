using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.ViewModels
{
    // Los umbrales se leen y se guardan en dbo.setting a través de RiskSettingsService.
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly RiskSettingsService _riskSettings;
        private readonly RiskConfig _config;

        public SettingsViewModel(RiskSettingsService riskSettings, RiskConfig config)
        {
            ArgumentNullException.ThrowIfNull(riskSettings);
            ArgumentNullException.ThrowIfNull(config);
            _riskSettings = riskSettings;
            _config = config;
            HighThreshold = config.HighThreshold.ToString(CultureInfo.InvariantCulture);
            MediumThreshold = config.MediumThreshold.ToString(CultureInfo.InvariantCulture);
        }

        [ObservableProperty]
        public partial string HighThreshold { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string MediumThreshold { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
        public partial string StatusMessage { get; set; } = string.Empty;

        // En false hasta terminar la lectura, así Guardar no pisa esa carga.
        [ObservableProperty]
        public partial bool CanSave { get; set; }

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        // Se dispara desde SettingsViewPage.OnAppearing.
        [RelayCommand]
        private async Task LoadAsync()
        {
            CanSave = false;
            try
            {
                await _riskSettings.LoadAsync();
                HighThreshold = _config.HighThreshold.ToString(CultureInfo.InvariantCulture);
                MediumThreshold = _config.MediumThreshold.ToString(CultureInfo.InvariantCulture);
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = "No se pudieron cargar los umbrales.";
                System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los umbrales: {ex}");
            }
            finally
            {
                CanSave = true;
            }
        }

        [RelayCommand]
        private async Task SaveThresholdsAsync()
        {
            try
            {
                var result = await _riskSettings.SaveAsync(HighThreshold, MediumThreshold);
                StatusMessage = result.Message;
            }
            catch (Exception ex)
            {
                StatusMessage = "No se pudieron guardar los umbrales.";
                System.Diagnostics.Debug.WriteLine($"No se pudieron guardar los umbrales: {ex}");
            }
        }
    }
}
