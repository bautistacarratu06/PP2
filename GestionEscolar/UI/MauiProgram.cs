using GestionEscolar.Core;
using GestionEscolar.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using UI.Services;
using UI.ViewModels;
using UI.Views;

namespace UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            WriteBootLog("create " + DateTime.Now.ToString("O"), append: false);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => WriteBootLog("unhandled " + e.ExceptionObject);
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(ConfigureFonts);

#if WINDOWS
            // FieldBorder / SearchBox son el único marco; sin esto WinUI dibuja un segundo borde.
            BorderlessInputs.Configure();
#endif

            builder.ConfigureLifecycleEvents(ConfigureLifecycle);
            LoadAppSettings(builder);

            builder.Services.AddGestionEscolarCore();
            builder.Services.AddGestionEscolarData(builder.Configuration);
            builder.Services.AddSingleton<SectionSelection>();

            // Costuras de UI: fecha, selector de CSV y navegación
            builder.Services.AddSingleton<IClock, SystemClock>();
            builder.Services.AddSingleton<ICsvFileSource, DeviceCsvFileSource>();
            builder.Services.AddSingleton<IAppNavigation, ShellAppNavigation>();

            // ViewModels
            builder.Services.AddTransient<CommissionImportFileStep>();
            builder.Services.AddTransient<StudentsViewModel>();
            builder.Services.AddTransient<StudentDetailViewModel>();
            builder.Services.AddTransient<CommissionImportViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();

            // Páginas (Shell las resuelve por DI, tanto en ShellContent como en rutas registradas)
            builder.Services.AddTransient<StudentsPage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<StudentDetailPage>();
            builder.Services.AddTransient<SettingsViewPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void ConfigureFonts(IFontCollection fonts)
        {
            fonts.AddFont("Inter-Regular.ttf", "Inter");
            fonts.AddFont("Inter-Medium.ttf", "InterMedium");
            fonts.AddFont("Inter-SemiBold.ttf", "InterSemibold");
            fonts.AddFont("Inter-Bold.ttf", "InterBold");
            fonts.AddFont("JetBrainsMono-Regular.ttf", "JetBrainsMono");
        }

        private static void ConfigureLifecycle(ILifecycleBuilder events)
        {
#if WINDOWS
            // Una sola vez: el color del caption no se ve si queda una franja transparente sobre Mica.
            events.AddWindows(windows => windows.OnWindowCreated(window =>
            {
                WriteBootLog("window-created");
                window.SystemBackdrop = null;
                PaintTitleBar(window.AppWindow.TitleBar);
                MaximizeWindow(window);
            }));
#endif
        }

#if WINDOWS
        private static void PaintTitleBar(global::Microsoft.UI.Windowing.AppWindowTitleBar titleBar)
        {
            var background = global::Windows.UI.Color.FromArgb(255, 0x0A, 0x0A, 0x0A);
            var foreground = global::Windows.UI.Color.FromArgb(255, 0x8E, 0x8E, 0x93);
            var inactiveForeground = global::Windows.UI.Color.FromArgb(255, 0x5C, 0x5C, 0x61);
            var hover = global::Windows.UI.Color.FromArgb(255, 0x1B, 0x1B, 0x1B);
            titleBar.BackgroundColor = background;
            titleBar.InactiveBackgroundColor = background;
            titleBar.ForegroundColor = foreground;
            titleBar.InactiveForegroundColor = inactiveForeground;
            titleBar.ButtonBackgroundColor = background;
            titleBar.ButtonInactiveBackgroundColor = background;
            titleBar.ButtonForegroundColor = foreground;
            titleBar.ButtonInactiveForegroundColor = inactiveForeground;
            titleBar.ButtonHoverBackgroundColor = hover;
            titleBar.ButtonHoverForegroundColor = foreground;
            titleBar.ButtonPressedBackgroundColor = hover;
            titleBar.ButtonPressedForegroundColor = foreground;
        }

        // Maximizada: llena el área de trabajo y deja la barra de título.
        private static void MaximizeWindow(Microsoft.UI.Xaml.Window window)
        {
            try
            {
                if (window.AppWindow.Presenter is global::Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    presenter.Maximize();
                    WriteBootLog("maximized");
                }
                else
                {
                    WriteBootLog("presenter=" + (window.AppWindow.Presenter?.GetType().FullName ?? "null"));
                }
            }
            catch (Exception ex)
            {
                WriteBootLog(ex.ToString());
            }
        }
#endif

        private static void WriteBootLog(string line, bool append = true)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt");
            var text = line + System.Environment.NewLine;
            if (append)
                System.IO.File.AppendAllText(path, text);
            else
                System.IO.File.WriteAllText(path, text);
        }

        // appsettings.json embebido. El valor de desarrollo apunta al SQL Server local
        // (.\SQLEXPRESS, base course_tracking, conexión de confianza).
        private static void LoadAppSettings(MauiAppBuilder builder)
        {
            using var resource = typeof(MauiProgram).Assembly.GetManifestResourceStream("UI.appsettings.json")
                ?? throw new InvalidOperationException("Falta appsettings.json.");

            var copy = new MemoryStream();
            resource.CopyTo(copy);
            copy.Position = 0;
            builder.Configuration.AddJsonStream(copy);
        }
    }
}