using GestionEscolar.Core.Interfaces;
using GestionEscolar.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Data;

// Alta de la conexión y de los repositorios que ya existen.
// La connection string llega por la configuración de la aplicación (no por dbo.setting).
public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddGestionEscolarData(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("CourseTracking") ?? string.Empty;
        services.AddSingleton(new DatabaseOptions { ConnectionString = connectionString });
        services.AddSingleton<DatabaseInstance>();

        services.AddSingleton<ITrackingRepository, SqlTrackingRepository>();
        services.AddSingleton<IRiskSummaryRepository, SqlRiskSummaryRepository>();
        services.AddSingleton<ISectionRepository, SqlSectionRepository>();
        services.AddSingleton<IStudentDetailsRepository, SqlStudentDetailsRepository>();
        services.AddSingleton<IStudentRepository, SqlStudentRepository>();
        services.AddSingleton<ICalendarRepository, SqlCalendarRepository>();
        services.AddSingleton<IAttendanceRepository, SqlAttendanceRepository>();
        services.AddSingleton<ISubmissionRepository, SqlSubmissionRepository>();
        return services;
    }
}
