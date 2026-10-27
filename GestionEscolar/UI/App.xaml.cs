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
            window.Created += (_, _) => ApplyTitleBar(window);
            return window;
        }

        private static void ApplyTitleBar(Window window)
        {
#if WINDOWS
            if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native)
                return;

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            var background = global::Windows.UI.Color.FromArgb(255, 10, 10, 10);
            var foreground = global::Windows.UI.Color.FromArgb(255, 142, 142, 147);
            var hover = global::Windows.UI.Color.FromArgb(255, 27, 27, 27);
            appWindow.Title = string.Empty;
            appWindow.TitleBar.BackgroundColor = background;
            appWindow.TitleBar.InactiveBackgroundColor = background;
            appWindow.TitleBar.ForegroundColor = foreground;
            appWindow.TitleBar.InactiveForegroundColor = foreground;
            appWindow.TitleBar.ButtonBackgroundColor = background;
            appWindow.TitleBar.ButtonInactiveBackgroundColor = background;
            appWindow.TitleBar.ButtonForegroundColor = foreground;
            appWindow.TitleBar.ButtonInactiveForegroundColor = foreground;
            appWindow.TitleBar.ButtonHoverBackgroundColor = hover;
            appWindow.TitleBar.ButtonHoverForegroundColor = foreground;
            appWindow.TitleBar.ButtonPressedBackgroundColor = hover;
            appWindow.TitleBar.ButtonPressedForegroundColor = foreground;
#endif
        }
    }
}
