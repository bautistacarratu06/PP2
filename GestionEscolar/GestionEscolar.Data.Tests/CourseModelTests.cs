using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Tests;

public class CourseModelTests
{
    [Fact]
    public void Core_does_not_reference_entity_framework()
    {
        var names = typeof(Section).Assembly.GetReferencedAssemblies().Select(assembly => assembly.Name);
        Assert.DoesNotContain(names, name => name is not null && name.Contains("EntityFramework", StringComparison.Ordinal));
    }

    [Fact]
    public void Model_maps_the_current_tables_and_keeps_the_risk_view_out()
    {
        var options = new DbContextOptionsBuilder<CourseTrackingDbContext>()
            .UseSqlServer("Server=.;Database=unused;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        using var db = new CourseTrackingDbContext(options);

        var tables = db.Model.GetEntityTypes().Select(entity => entity.GetTableName()!).OrderBy(name => name).ToArray();
        Assert.Equal(
            ["assignment", "attendance_record", "class_session", "intervention", "section", "setting", "student", "submission"],
            tables);

        var student = db.Model.FindEntityType(typeof(StudentEntity));
        Assert.NotNull(student);
        Assert.Equal(nameof(StudentEntity.Dni), student.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Contains(student.GetKeys(), key => key.Properties.Single().Name == nameof(StudentEntity.Id));
        Assert.Contains(student.GetIndexes(), index => index.GetDatabaseName() == "ix_student_section_id");
    }
}
