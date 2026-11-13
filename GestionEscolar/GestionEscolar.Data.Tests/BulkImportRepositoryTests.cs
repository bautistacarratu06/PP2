using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Tests;

[Collection("sqlserver")]
public sealed class BulkImportRepositoryTests(SqlServerFixture fixture)
{
    private static readonly DateTime Enrolled = new(2026, 10, 27);

    private readonly SqlServerFixture _fixture = fixture;

    private ICommissionImportRepository Repository => _fixture.Get<ICommissionImportRepository>();

    private async Task<int> SeedSectionAsync(string name)
    {
        await using var db = _fixture.CreateContext();
        var section = new SectionEntity { Name = name };
        db.Sections.Add(section);
        await db.SaveChangesAsync();
        return section.Id;
    }

    private static List<NumberedStudent> Rows(int count, string prefix = "D", int firstLine = 2) =>
        Enumerable.Range(0, count)
            .Select(i => new NumberedStudent(firstLine + i, new Student
            {
                Dni = $"{prefix}{i}",
                StudentNumber = $"L{i}",
                LastName = $"Apellido{i}",
                FirstName = $"Nombre{i}",
                Phone = i % 2 == 0 ? $"11{i}" : string.Empty,
                Email = $"a{i}@edu.test",
                EnrolledOn = Enrolled
            }))
            .ToList();

    private async Task SeedStoredAsync(int sectionId, IEnumerable<string> dnis)
    {
        await using var db = _fixture.CreateContext();
        foreach (var dni in dnis)
        {
            db.Students.Add(new StudentEntity
            {
                SectionId = sectionId,
                Dni = dni,
                StudentNumber = "S" + dni,
                LastName = "Guardado",
                FirstName = "Previo",
                EnrolledOn = Enrolled
            });
        }

        await db.SaveChangesAsync();
    }

    private async Task<List<string>> RosterAsync(int sectionId)
    {
        await using var db = _fixture.CreateContext();
        var rows = await db.Students
            .Where(student => student.SectionId == sectionId)
            .OrderBy(student => student.StudentNumber)
            .Select(student => new { student.StudentNumber, student.LastName, student.FirstName, student.Phone, student.Email, student.EnrolledOn })
            .ToListAsync();
        return rows.Select(r => $"{r.StudentNumber}|{r.LastName}|{r.FirstName}|{r.Phone}|{r.Email}|{r.EnrolledOn:yyyy-MM-dd}").ToList();
    }

    private async Task<int> CountAsync(int sectionId)
    {
        await using var db = _fixture.CreateContext();
        return await db.Students.CountAsync(student => student.SectionId == sectionId);
    }

    [Fact]
    public async Task Given_1000Rows_When_Applying_Then_AllAreStored()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Mil");

        await Repository.ApplyAsync(sectionId, Rows(1000).Select(row => row.Student).ToList(), []);

        Assert.Equal(1000, await CountAsync(sectionId));
    }

    [Fact]
    public async Task Given_AConstraintViolationInALaterBatch_When_Applying_Then_NothingIsCommitted()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Falla");
        var students = Rows(450).Select(row => row.Student).ToList();
        students[430] = new Student
        {
            Dni = students[3].Dni,
            StudentNumber = "OTRO",
            LastName = "Duplicado",
            FirstName = "Dni",
            EnrolledOn = Enrolled
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => Repository.ApplyAsync(sectionId, students, []));

        Assert.Equal(0, await CountAsync(sectionId));
    }

    [Fact]
    public async Task Given_988ValidAnd12DuplicateRows_When_ApplyingWithReport_Then_ValidOnesAreStoredAndErrorsNameTheLine()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Mixta");
        var rows = Rows(1000);
        var duplicated = Enumerable.Range(0, 12).Select(i => i * 80).ToList();
        await SeedStoredAsync(sectionId, duplicated.Select(i => rows[i].Student.Dni));

        var report = await Repository.ApplyWithReportAsync(sectionId, rows, []);

        Assert.Equal(988, report.Inserted);
        Assert.Equal(duplicated.Select(i => rows[i].Line), report.Errors.Select(e => e.Line).OrderBy(line => line));
        Assert.All(report.Errors, error => Assert.False(string.IsNullOrWhiteSpace(error.Message)));
        Assert.Equal(1000, await CountAsync(sectionId));
    }

    [Fact]
    public async Task Given_AllValidRows_When_ApplyingWithReport_Then_AllInsertedAndNoErrors()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Limpia");

        var report = await Repository.ApplyWithReportAsync(sectionId, Rows(250), []);

        Assert.Equal(250, report.Inserted);
        Assert.Empty(report.Errors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_EveryRowConflicts_When_Applying_Then_NothingIsInsertedAndEveryLineIsReported(bool bulk)
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Todas");
        var rows = Rows(30);
        await SeedStoredAsync(sectionId, rows.Select(row => row.Student.Dni));

        var report = bulk
            ? await Repository.BulkApplyAsync(sectionId, rows, [])
            : await Repository.ApplyWithReportAsync(sectionId, rows, []);

        Assert.Equal(0, report.Inserted);
        Assert.Equal(rows.Select(row => row.Line), report.Errors.Select(e => e.Line).OrderBy(line => line));
        Assert.Equal(30, await CountAsync(sectionId));
    }

    [Fact]
    public async Task Given_TheSameInput_When_UsingBulkAndNormalPaths_Then_StoredRostersAndReportsMatch()
    {
        await _fixture.ClearRowsAsync();
        var normalId = await SeedSectionAsync("Normal");
        var bulkId = await SeedSectionAsync("Masiva");
        var normalRows = Rows(600, "N");
        var bulkRows = Rows(600, "B");
        // Mismas filas con DNI distintos (el DNI es único en toda la base); misma duplicación interna.
        foreach (var rows in new[] { normalRows, bulkRows })
        {
            rows[10] = rows[10] with { Student = Copy(rows[10].Student, rows[5].Student.Dni, rows[10].Student.StudentNumber) };
            rows[20] = rows[20] with { Student = Copy(rows[20].Student, rows[20].Student.Dni, rows[7].Student.StudentNumber) };
        }

        var normal = await Repository.ApplyWithReportAsync(normalId, normalRows, []);
        var bulk = await Repository.BulkApplyAsync(bulkId, bulkRows, []);

        Assert.Equal(normal.Inserted, bulk.Inserted);
        Assert.Equal(598, bulk.Inserted);
        Assert.Equal(normal.Errors.Select(e => e.Line), bulk.Errors.Select(e => e.Line));
        Assert.Equal(await RosterAsync(normalId), await RosterAsync(bulkId));
    }

    [Fact]
    public async Task Given_ALargeImportThroughTheService_When_Confirming_Then_ConflictsAreReportedPerLine()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Servicio");
        await SeedStoredAsync(sectionId, ["500", "501"]);
        var service = new CommissionImportService(Repository);
        var lines = new List<string> { "DNI,Apellido,Nombre,Legajo,Teléfono,Correo" };
        lines.AddRange(Enumerable.Range(0, 400).Select(i => $"{i},Ap{i},Nom{i},L{i},,c{i}@edu.test"));
        lines.Add("500,Ap,Nom,L900,,");
        var rows = service.Read(new StringReader(string.Join('\n', lines)));
        var preview = await service.PreviewAsync(sectionId, rows);

        var report = await service.ApplyWithReportAsync(sectionId, preview, preview.Conflicts.Select(_ => ImportDecision.Omit).ToList(), DateTime.Today);

        Assert.Single(preview.Conflicts);
        Assert.Equal(400, report.Inserted);
        Assert.Equal(402, await CountAsync(sectionId));
    }

    [Fact]
    public async Task Given_ALargeNewSection_When_CreatingFromThePreview_Then_ItUsesTheBulkPathAndReportsProgress()
    {
        await _fixture.ClearRowsAsync();
        await SeedStoredAsync(await SeedSectionAsync("Previa"), ["7"]);
        var service = new CommissionImportService(Repository);
        var lines = new List<string> { "DNI,Apellido,Nombre,Legajo,Teléfono,Correo" };
        lines.AddRange(Enumerable.Range(0, 450).Select(i => $"{i},Ap{i},Nom{i},L{i},,c{i}@edu.test"));
        var rows = service.Read(new StringReader(string.Join('\n', lines)));
        var preview = await service.PreviewAsync(null, rows);
        var progress = new RecordingProgress();

        var section = await service.CreateFromPreviewAsync("Grande", preview, DateTime.Today, progress);

        Assert.Single(preview.Conflicts);
        Assert.Equal(449, await CountAsync(section.Id));
        Assert.Contains(progress.Values, value => value.Stage == ImportStage.Saving && value.Done == 449 && value.Total == 449);
        Assert.Equal(ImportStage.Finishing, progress.Values[^1].Stage);
        await using var db = _fixture.CreateContext();
        Assert.Equal(0, await db.Database.SqlQuery<int>($"SELECT COUNT(*) AS [Value] FROM dbo.student_import_staging").SingleAsync());
    }

    // Progress<T> publica en otro hilo; este guarda los avisos en orden y en el acto.
    private sealed class RecordingProgress : IProgress<ImportProgress>
    {
        public List<ImportProgress> Values { get; } = [];

        public void Report(ImportProgress value) => Values.Add(value);
    }

    private static Student Copy(Student student, string dni, string number) => new()
    {
        Dni = dni,
        StudentNumber = number,
        LastName = student.LastName,
        FirstName = student.FirstName,
        Phone = student.Phone,
        Email = student.Email,
        EnrolledOn = student.EnrolledOn
    };
}
