using GestionEscolar.Data.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Tests;

public class ExistingSchemaMigrationTests
{
    [Fact]
    public async Task Given_HistoricalSchemaWithRows_When_Updating_Then_RowsStayAndTheIndexAppears()
    {
        const string databaseName = "course_tracking_ef_legacy";
        var connectionString =
            $@"Server=.\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

        await SqlServerAdmin.DropAsync(databaseName);
        try
        {
            var options = new DbContextOptionsBuilder<CourseTrackingDbContext>().UseSqlServer(connectionString).Options;
            await using (var db = new CourseTrackingDbContext(options))
            {
                await db.Database.MigrateAsync();
                db.Sections.Add(new SectionEntity { Name = "Historica" });
                await db.SaveChangesAsync();
                db.Students.Add(new StudentEntity
                {
                    SectionId = db.Sections.Single().Id,
                    Dni = "30111222",
                    StudentNumber = "15",
                    LastName = "Paz",
                    FirstName = "Nora"
                });
                await db.SaveChangesAsync();
            }

            await ExecuteAsync(connectionString, "DROP INDEX ix_student_section_id ON dbo.student; DELETE FROM dbo.__EFMigrationsHistory;");

            var sectionsBefore = await ScalarAsync<int>(connectionString, "SELECT COUNT(*) FROM dbo.section");
            var studentsBefore = await ScalarAsync<int>(connectionString, "SELECT COUNT(*) FROM dbo.student");

            await using (var db = new CourseTrackingDbContext(options))
                await db.Database.MigrateAsync();

            Assert.Equal(sectionsBefore, await ScalarAsync<int>(connectionString, "SELECT COUNT(*) FROM dbo.section"));
            Assert.Equal(studentsBefore, await ScalarAsync<int>(connectionString, "SELECT COUNT(*) FROM dbo.student"));
            Assert.Equal("Historica", await ScalarAsync<string>(connectionString, "SELECT name FROM dbo.section"));
            Assert.Equal(1, await ScalarAsync<int>(connectionString, "SELECT COUNT(*) FROM dbo.setting WHERE setting_key = N'high_threshold'"));
            Assert.NotNull(await ScalarAsync<object>(
                connectionString,
                "SELECT 1 FROM sys.indexes WHERE name = N'ix_student_section_id' AND object_id = OBJECT_ID(N'dbo.student')"));
        }
        finally
        {
            await SqlServerAdmin.DropAsync(databaseName);
        }
    }

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T?> ScalarAsync<T>(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync();
        if (value is null or DBNull)
            return default;
        return (T)value;
    }
}
