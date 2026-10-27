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

            return new Window(new AppShell())
            {
                // * este es nuestro entrypoint que monta las shells
                // Shell muestra el primer ShellContent. tenemos q tenerlo en cuenta 
                // para la navegación. Investigar como hacer 1 sidebar
                Title = "Gestión Escolar PP2",
                Width = 1360,
                Height = 900
            };
        }
    }
}
