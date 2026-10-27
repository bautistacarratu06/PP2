using System.Diagnostics;
using GestionEscolar.Core.Services;

namespace UI
{
    public partial class App : Application
    {
        private readonly RiskSettingsService _riskSettings;

        public App(RiskSettingsService riskSettings)
        {
            ArgumentNullException.ThrowIfNull(riskSettings);
            _riskSettings = riskSettings;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Seguimiento clasifica con el RiskConfig en memoria. Hay que llenarlo desde
            // la tabla antes de abrir el Shell; si la base no responde, quedan 50 y 25.
            try
            {
                Task.Run(async () => await _riskSettings.LoadAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"No se pudieron cargar los umbrales: {ex}");
            }

            var window = new Window(new AppShell())
            {
                Title = string.Empty,
                Width = 1360,
                Height = 900,
                MinimumWidth = 900,
                MinimumHeight = 640
            };
#if WINDOWS
            // Sin título: el nombre vive en el sidebar. El fondo es Bg, no el gris del sistema.
            window.TitleBar = new TitleBar
            {
                BackgroundColor = Color.FromArgb("#0A0A0A")
            };
#endif
            return window;
        }
    }
}
