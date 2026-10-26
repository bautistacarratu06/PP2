using GestionEscolar.Core;
using GestionEscolar.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UI.ViewModels;
using UI.Views;

namespace UI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            LoadAppSettings(builder);

            builder.Services.AddGestionEscolarCore();
            builder.Services.AddGestionEscolarData(builder.Configuration);

            // ViewModels
            builder.Services.AddTransient<TrackingViewModel>();
            builder.Services.AddTransient<StudentDetailViewModel>();
            builder.Services.AddTransient<StudentListViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<AttendanceViewModel>();
            builder.Services.AddTransient<SubmissionsViewModel>();
            builder.Services.AddTransient<CalendarViewModel>();

            // Páginas (Shell las resuelve por DI, tanto en ShellContent como en rutas registradas)
            builder.Services.AddTransient<MainPage>();
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