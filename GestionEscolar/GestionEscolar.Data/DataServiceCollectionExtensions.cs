using GestionEscolar.Core.Interfaces;
using GestionEscolar.Data.Persistence;
using GestionEscolar.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Data;

// Alta de la conexión y de los repositorios.
// La connection string llega por la configuración de la aplicación (no por dbo.setting).
public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddGestionEscolarData(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("CourseTracking") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Falta la connection string.", nameof(configuration));

        services.AddSingleton(new DatabaseOptions { ConnectionString = connectionString });
        services.AddSingleton<DatabaseInstance>();
        services.AddDbContextFactory<CourseTrackingDbContext>(options => options.UseSqlServer(connectionString));

        services.AddSingleton<ITrackingRepository, EfTrackingRepository>();
        services.AddSingleton<IRiskSummaryRepository, EfRiskSummaryRepository>();
        services.AddSingleton<IRiskSettingsRepository, EfRiskSettingsRepository>();
        services.AddSingleton<ISectionRepository, EfSectionRepository>();
        services.AddSingleton<IStudentDetailsRepository, EfStudentDetailsRepository>();
        services.AddSingleton<IStudentRepository, EfStudentRepository>();
        services.AddSingleton<ICommissionImportRepository, EfCommissionImportRepository>();
        services.AddSingleton<ICalendarRepository, EfCalendarRepository>();
        services.AddSingleton<IAttendanceRepository, EfAttendanceRepository>();
        services.AddSingleton<ISubmissionRepository, EfSubmissionRepository>();
        return services;
    }
}
