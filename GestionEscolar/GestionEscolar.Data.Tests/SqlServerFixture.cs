using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using GestionEscolar.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Data.Tests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string DatabaseName = "course_tracking_ef_test";

    public string ConnectionString { get; } =
        $@"Server=.\SQLEXPRESS;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    private ServiceProvider _services = null!;

    public async Task InitializeAsync()
    {
        await SqlServerAdmin.DropAsync(DatabaseName);
        var services = new ServiceCollection();
        services.AddDbContextFactory<CourseTrackingDbContext>(options => options.UseSqlServer(ConnectionString));
        services.AddSingleton(new RiskConfig());
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
        _services = services.BuildServiceProvider();

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _services.DisposeAsync();
        await SqlServerAdmin.DropAsync(DatabaseName);
    }

    public T Get<T>() where T : notnull => _services.GetRequiredService<T>();

    public CourseTrackingDbContext CreateContext() =>
        _services.GetRequiredService<IDbContextFactory<CourseTrackingDbContext>>().CreateDbContext();

    public async Task ClearRowsAsync()
    {
        await using var db = CreateContext();
        await db.AttendanceRecords.ExecuteDeleteAsync();
        await db.Submissions.ExecuteDeleteAsync();
        await db.Interventions.ExecuteDeleteAsync();
        await db.ClassSessions.ExecuteDeleteAsync();
        await db.Assignments.ExecuteDeleteAsync();
        await db.Students.ExecuteDeleteAsync();
        await db.Sections.ExecuteDeleteAsync();
    }
}

[CollectionDefinition("sqlserver")]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>;

public static class SqlServerAdmin
{
    public static async Task DropAsync(string databaseName)
    {
        const string master = @"Server=.\SQLEXPRESS;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
        await using var connection = new SqlConnection(master);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF DB_ID(@name) IS NOT NULL
            BEGIN
                DECLARE @sql nvarchar(max) = N'ALTER DATABASE ' + QUOTENAME(@name) + N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE ' + QUOTENAME(@name) + N';';
                EXEC(@sql);
            END
            """;
        command.Parameters.Add(new SqlParameter("@name", databaseName));
        await command.ExecuteNonQueryAsync();
    }
}
