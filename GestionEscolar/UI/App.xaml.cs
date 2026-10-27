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
            window.Activated += (_, _) => ApplyTitleBar(window);
            return window;
        }

        private static void ApplyTitleBar(Window window)
        {
#if WINDOWS
            if (window.Handler?.PlatformView is not Microsoft.UI.Xaml.Window native)
                return;

            var background = global::Windows.UI.Color.FromArgb(255, 10, 10, 10);
            var foreground = global::Windows.UI.Color.FromArgb(255, 142, 142, 147);
            var hover = global::Windows.UI.Color.FromArgb(255, 27, 27, 27);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(native);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var titleBar = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId).TitleBar;
            titleBar.BackgroundColor = background;
            titleBar.InactiveBackgroundColor = background;
            titleBar.ForegroundColor = foreground;
            titleBar.InactiveForegroundColor = foreground;
            titleBar.ButtonBackgroundColor = background;
            titleBar.ButtonInactiveBackgroundColor = background;
            titleBar.ButtonForegroundColor = foreground;
            titleBar.ButtonInactiveForegroundColor = foreground;
            titleBar.ButtonHoverBackgroundColor = hover;
            titleBar.ButtonHoverForegroundColor = foreground;
            titleBar.ButtonPressedBackgroundColor = hover;
            titleBar.ButtonPressedForegroundColor = foreground;

            // El caption de WinUI usa el pincel del tema, más claro que Bg. Hay que pintarlo aparte.
            var caption = 0x000A0A0A;
            var text = 0x00938E8E;
            DwmSetWindowAttribute(hwnd, 35, ref caption, sizeof(int));
            DwmSetWindowAttribute(hwnd, 34, ref caption, sizeof(int));
            DwmSetWindowAttribute(hwnd, 36, ref text, sizeof(int));

            var brush = new Microsoft.UI.Xaml.Media.SolidColorBrush(background);
            if (native.Content is Microsoft.UI.Xaml.Controls.Grid root)
            {
                root.Background = brush;
                foreach (var child in root.Children)
                {
                    if (child is Microsoft.UI.Xaml.FrameworkElement element
                        && Microsoft.UI.Xaml.Controls.Grid.GetRow(element) == 0)
                        Paint(element, brush);
                }
            }

            if (native.Content is Microsoft.UI.Xaml.DependencyObject tree)
                PaintNamedTitleBar(tree, brush);
#endif
        }

#if WINDOWS
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        private static void PaintNamedTitleBar(Microsoft.UI.Xaml.DependencyObject node, Microsoft.UI.Xaml.Media.Brush brush)
        {
            if (node is Microsoft.UI.Xaml.FrameworkElement element
                && element.Name.Contains("TitleBar", StringComparison.Ordinal))
                Paint(element, brush);

            var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(node);
            for (var i = 0; i < count; i++)
                PaintNamedTitleBar(Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(node, i), brush);
        }

        private static void Paint(Microsoft.UI.Xaml.FrameworkElement element, Microsoft.UI.Xaml.Media.Brush brush)
        {
            switch (element)
            {
                case Microsoft.UI.Xaml.Controls.Panel panel:
                    panel.Background = brush;
                    break;
                case Microsoft.UI.Xaml.Controls.Control control:
                    control.Background = brush;
                    break;
                case Microsoft.UI.Xaml.Controls.Border border:
                    border.Background = brush;
                    break;
            }
        }
#endif
    }
}
