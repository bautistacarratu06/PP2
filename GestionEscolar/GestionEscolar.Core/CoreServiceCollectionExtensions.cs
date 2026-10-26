using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Core;

// Alta de los casos de uso. Los umbrales siguen en RiskConfig (50/25): este corte
// no agrega un puerto para leerlos de dbo.setting.
public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddGestionEscolarCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(new RiskConfig());
        services.AddTransient<TrackingService>();
        services.AddTransient<SectionService>();
        services.AddTransient<StudentDetailsService>();
        services.AddTransient<StudentService>();
        services.AddTransient<CalendarService>();
        services.AddTransient<AttendanceService>();
        services.AddTransient<SubmissionService>();
        return services;
    }
}
