using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels
{
    // TODO: Solo UI por ahora. Los umbrales están hardcodeados y no se persisten.
    // Más adelante conectar con la base de datos (high_threshold, medium_threshold)
    // y con RiskRule, que hoy también los tiene fijos en 50 y 25.
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string HighThreshold { get; set; } = "50";

        [ObservableProperty]
        public partial string MediumThreshold { get; set; } = "25";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasStatusMessage))]
        public partial string StatusMessage { get; set; } = string.Empty;

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        [RelayCommand]
        private void SaveThresholds()
        {
            // Sin persistencia todavía: solo validamos y avisamos.
            if (!int.TryParse(HighThreshold, out var high) || !int.TryParse(MediumThreshold, out var medium))
            {
                StatusMessage = "Ingresá valores numéricos.";
                return;
            }

            if (high is < 0 or > 100 || medium is < 0 or > 100 || medium >= high)
            {
                StatusMessage = "Los valores deben estar entre 0 y 100, y Medio debe ser menor que Alto.";
                return;
            }

            StatusMessage = "Umbrales válidos (todavía no se guardan).";
        }
    }
}