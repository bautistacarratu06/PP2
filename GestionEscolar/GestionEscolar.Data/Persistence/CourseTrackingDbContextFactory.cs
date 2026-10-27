using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestionEscolar.Data.Persistence;

// Solo lo usa `dotnet ef`. La aplicación lee ConnectionStrings:CourseTracking.
public sealed class CourseTrackingDbContextFactory : IDesignTimeDbContextFactory<CourseTrackingDbContext>
{
    public const string LocalConnectionString =
        @"Server=.\SQLEXPRESS;Database=course_tracking;Trusted_Connection=True;TrustServerCertificate=True;";

    public CourseTrackingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("COURSE_TRACKING_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = LocalConnectionString;

        var options = new DbContextOptionsBuilder<CourseTrackingDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new CourseTrackingDbContext(options);
    }
}
