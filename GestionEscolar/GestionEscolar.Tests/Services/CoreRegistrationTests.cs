using GestionEscolar.Core;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Tests.Services;

public class CoreRegistrationTests
{
    [Fact]
    public void Given_CoreRegistration_When_InspectingLifetimes_Then_UseCasesAreTransientAndThresholdsAreSingleton()
    {
        var services = new ServiceCollection();

        services.AddGestionEscolarCore();

        Assert.Equal(ServiceLifetime.Singleton, Single(services, typeof(RiskConfig)).Lifetime);
        Assert.All(
            new[]
            {
                typeof(TrackingService),
                typeof(SectionService),
                typeof(StudentDetailsService),
                typeof(StudentService),
                typeof(CalendarService),
                typeof(AttendanceService),
                typeof(SubmissionService)
            },
            type => Assert.Equal(ServiceLifetime.Transient, Single(services, type).Lifetime));
    }

    private static ServiceDescriptor Single(IServiceCollection services, Type serviceType) =>
        Assert.Single(services, descriptor => descriptor.ServiceType == serviceType);
}
