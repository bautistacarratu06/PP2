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
            System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt"), "create " + DateTime.Now.ToString("O") + System.Environment.NewLine);
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt"), "unhandled " + e.ExceptionObject + System.Environment.NewLine);
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Inter-Regular.ttf", "Inter");
                    fonts.AddFont("Inter-Medium.ttf", "InterMedium");
                    fonts.AddFont("Inter-SemiBold.ttf", "InterSemibold");
                    fonts.AddFont("Inter-Bold.ttf", "InterBold");
                    fonts.AddFont("JetBrainsMono-Regular.ttf", "JetBrainsMono");
                });

#if WINDOWS
            // FieldBorder / SearchBox son el único marco; sin esto WinUI dibuja un segundo borde.
            BorderlessInputs.Configure();
#endif

            builder
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    // Una sola vez: el color del caption no se ve si queda una franja transparente sobre Mica.
                    events.AddWindows(windows => windows.OnWindowCreated(window =>
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt"), "window-created" + System.Environment.NewLine);
                        window.SystemBackdrop = null;

                        var background = global::Windows.UI.Color.FromArgb(255, 0x0A, 0x0A, 0x0A);
                        var foreground = global::Windows.UI.Color.FromArgb(255, 0x8E, 0x8E, 0x93);
                        var inactiveForeground = global::Windows.UI.Color.FromArgb(255, 0x5C, 0x5C, 0x61);
                        var hover = global::Windows.UI.Color.FromArgb(255, 0x1B, 0x1B, 0x1B);
                        var titleBar = window.AppWindow.TitleBar;
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

                        // Maximizada: llena el área de trabajo y deja la barra de título.
                        try
                        {
                            if (window.AppWindow.Presenter is global::Microsoft.UI.Windowing.OverlappedPresenter presenter)
                            {
                                presenter.Maximize();
                                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt"), "maximized" + System.Environment.NewLine);
                            }
                            else
                            {
                                System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt"), "presenter=" + (window.AppWindow.Presenter?.GetType().FullName ?? "null") + System.Environment.NewLine);
                            }
                        }
                        catch (Exception ex)
                        {
                            System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "ui-boot.txt"), ex + System.Environment.NewLine);
                        }
                    }));
#endif
                });

            LoadAppSettings(builder);

            builder.Services.AddGestionEscolarCore();
            builder.Services.AddGestionEscolarData(builder.Configuration);
            builder.Services.AddSingleton<SectionSelection>();

            // ViewModels
            builder.Services.AddTransient<TrackingViewModel>();
            builder.Services.AddTransient<StudentDetailViewModel>();
            builder.Services.AddTransient<StudentListViewModel>();
            builder.Services.AddTransient<CommissionImportViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<AttendanceViewModel>();
            builder.Services.AddTransient<SubmissionsViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();

            // Páginas (Shell las resuelve por DI, tanto en ShellContent como en rutas registradas)
            builder.Services.AddTransient<TrackingPage>();
            builder.Services.AddTransient<SubmissionsPage>();
            builder.Services.AddTransient<AttendancePage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<StudentDetailPage>();
            builder.Services.AddTransient<StudentListPage>();
            builder.Services.AddTransient<SettingsViewPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
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