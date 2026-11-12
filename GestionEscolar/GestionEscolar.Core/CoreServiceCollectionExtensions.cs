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
        AddWithContract<ITrackingService, TrackingService>(services);
        AddWithContract<IStudentsQueryService, StudentsQueryService>(services);
        AddWithContract<IRiskSettingsService, RiskSettingsService>(services);
        AddWithContract<ISectionService, SectionService>(services);
        AddWithContract<IStudentDetailsService, StudentDetailsService>(services);
        AddWithContract<IStudentService, StudentService>(services);
        AddWithContract<ICommissionImportService, CommissionImportService>(services);
        AddWithContract<ICalendarService, CalendarService>(services);
        AddWithContract<IAttendanceService, AttendanceService>(services);
        AddWithContract<ISubmissionService, SubmissionService>(services);
        return services;
    }

    // Se mantiene el tipo concreto registrado y la interfaz resuelve esa misma instancia transitoria.
    private static void AddWithContract<TContract, TImplementation>(IServiceCollection services)
        where TContract : class
        where TImplementation : class, TContract
    {
        services.AddTransient<TImplementation>();
        services.AddTransient<TContract>(sp => sp.GetRequiredService<TImplementation>());
    }
}
