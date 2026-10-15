using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Data;
using GestionEscolar.Data.Repositories;
using Microsoft.Extensions.Logging;
using UI.ViewModels;
using UI.Views;

namespace UI
{
    public static class MauiProgram
    {
        // Único lugar donde se define la conexión a la base.
        // TODO: moverla a un appsettings.json embebido si hace falta cambiarla por entorno.
        private const string ConnectionString =
            "Server=.\\SQLEXPRESS;Database=course_tracking;Trusted_Connection=True;TrustServerCertificate=True;";

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

            // Configuración
            builder.Services.AddSingleton(new DatabaseOptions { ConnectionString = ConnectionString });
            builder.Services.AddSingleton(new RiskConfig()); // 50 / 25, hardcodeado por ahora

            // Datos
            builder.Services.AddSingleton<DatabaseInstance>();
            builder.Services.AddSingleton<ITrackingRepository, SqlTrackingRepository>();
            builder.Services.AddSingleton<IRiskSummaryRepository, SqlRiskSummaryRepository>();
            builder.Services.AddSingleton<ISectionRepository, SqlSectionRepository>();

            // New Student
            builder.Services.AddSingleton<IStudentRepository, SqlStudentRepository>();

            // Servicios
            builder.Services.AddTransient<TrackingService>();
            builder.Services.AddTransient<SectionService>();

            // New Student
            builder.Services.AddTransient<StudentService>();

            // ViewModels
            builder.Services.AddTransient<TrackingViewModel>();
            builder.Services.AddTransient<StudentDetailViewModel>();
            builder.Services.AddTransient<StudentListViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            // Páginas (Shell las resuelve por DI, tanto en ShellContent como en rutas registradas)
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<TrackingPage>();
            builder.Services.AddTransient<StudentDetailPage>();
            builder.Services.AddTransient<StudentListPage>();
            builder.Services.AddTransient<SettingsViewPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}