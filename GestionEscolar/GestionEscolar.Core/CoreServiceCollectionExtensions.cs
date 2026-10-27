using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Core;

// Alta de los casos de uso. RiskConfig es el umbral en memoria (singleton);
// RiskSettingsService lo carga y lo guarda a través del puerto.
public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddGestionEscolarCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(new RiskConfig());
        services.AddTransient<TrackingService>();
        services.AddTransient<RiskSettingsService>();
        services.AddTransient<SectionService>();
        services.AddTransient<StudentDetailsService>();
        services.AddTransient<StudentService>();
        services.AddTransient<CommissionImportService>();
        services.AddTransient<CalendarService>();
        services.AddTransient<AttendanceService>();
        services.AddTransient<SubmissionService>();
        return services;
    }
}
