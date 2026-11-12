using System.Globalization;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class BulkImportServiceTests
{
    private static readonly DateTime Today = new(2026, 10, 27);

    private static string Csv(int rows, Func<int, (string Dni, string Number)> key)
    {
        var lines = new List<string> { "DNI,Apellido,Nombre,Legajo,Teléfono,Correo" };
        for (var i = 0; i < rows; i++)
        {
            var (dni, number) = key(i);
            lines.Add($"{dni},Apellido{i},Nombre{i},{number},11{i:D6},a{i}@edu.test");
        }

        return string.Join('\n', lines);
    }

    // Implementación lineal anterior, como referencia del comportamiento.
    private static ImportPreview LinearPreview(int? sectionId, IReadOnlyList<RosterCsvRow> rows, IReadOnlyList<Student> students)
    {
        var inserts = new List<RosterCsvRow>();
        var conflicts = new List<ImportConflict>();
        var seenDni = new HashSet<string>(StringComparer.Ordinal);
        var seenNumbers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var byDni = students.FirstOrDefault(student => student.Dni == row.Dni);
            var byNumber = sectionId is int section
                ? students.FirstOrDefault(student => student.SectionId == section && student.StudentNumber == row.StudentNumber)
                : null;
            var dniInFile = !seenDni.Add(row.Dni);
            var numberInFile = !seenNumbers.Add(row.StudentNumber);
            var dniClash = byDni is not null || dniInFile;
            var numberClash = byNumber is not null || numberInFile;
            if (!dniClash && !numberClash)
            {
                inserts.Add(row);
                continue;
            }

            var hitsDatabase = byDni is not null || byNumber is not null;
            var hitsFile = dniInFile || numberInFile;
            var sameStudent = byDni is not null && byNumber is not null && byDni.Id == byNumber.Id;
            conflicts.Add(new ImportConflict
            {
                Row = row,
                Criterion = dniClash && numberClash ? ConflictCriterion.Both : dniClash ? ConflictCriterion.Dni : ConflictCriterion.StudentNumber,
                Origin = hitsDatabase && hitsFile ? ConflictOrigin.DatabaseAndFile : hitsDatabase ? ConflictOrigin.Database : ConflictOrigin.File,
                CanMerge = sameStudent,
                StudentId = sameStudent ? byDni!.Id : null
            });
        }

        return new ImportPreview(inserts, conflicts);
    }

    [Fact]
    public async Task Given_1000RowsAndAPopulatedRoster_When_Previewing_Then_ResultMatchesTheLinearImplementation()
    {
        var repo = new FakeCommissionImportRepository();
        var stored = Enumerable.Range(0, 1000).Select(i => new Student
        {
            Dni = $"S{i}",
            StudentNumber = $"SL{i}",
            LastName = "Guardado",
            FirstName = i.ToString(CultureInfo.InvariantCulture),
            Name = "x"
        }).ToArray();
        repo.Seed(new Section(1, "Mañana"), stored);
        var service = new CommissionImportService(repo);
        // Cada 7 colisiona por DNI, cada 11 por legajo, cada 13 repite el DNI de la fila anterior.
        var csv = Csv(1000, i => (
            i % 13 == 5 ? $"N{i - 1}" : i % 7 == 0 ? $"S{i}" : $"N{i}",
            i % 11 == 0 ? $"SL{i}" : $"NL{i}"));
        var rows = service.Read(new StringReader(csv));

        var preview = await service.PreviewAsync(1, rows);
        var expected = LinearPreview(1, rows, await repo.GetStudentsAsync());

        Assert.Equal(expected.ToInsert.Select(r => r.Line), preview.ToInsert.Select(r => r.Line));
        Assert.Equal(
            expected.Conflicts.Select(c => (c.Row.Line, c.Criterion, c.Origin, c.CanMerge, c.StudentId)),
            preview.Conflicts.Select(c => (c.Row.Line, c.Criterion, c.Origin, c.CanMerge, c.StudentId)));
        Assert.NotEmpty(preview.Conflicts);
        Assert.NotEmpty(preview.ToInsert);
    }

    [Fact]
    public async Task Given_RowsBelowTheThreshold_When_ApplyingWithReport_Then_TheNormalPathIsUsed()
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(new Section(1, "Mañana"));
        var service = new CommissionImportService(repo);
        var rows = service.Read(new StringReader(Csv(CommissionImportService.BulkThreshold - 1, i => ($"N{i}", $"L{i}"))));
        var preview = await service.PreviewAsync(1, rows);

        var report = await service.ApplyWithReportAsync(1, preview, [], Today);

        Assert.Equal(1, repo.ApplyWithReportCalls);
        Assert.Equal(0, repo.BulkApplyCalls);
        Assert.Equal(CommissionImportService.BulkThreshold - 1, report.Inserted);
        Assert.Empty(report.Errors);
    }

    [Fact]
    public async Task Given_RowsAtTheThreshold_When_ApplyingWithReport_Then_TheBulkPathIsUsed()
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(new Section(1, "Mañana"));
        var service = new CommissionImportService(repo);
        var rows = service.Read(new StringReader(Csv(CommissionImportService.BulkThreshold, i => ($"N{i}", $"L{i}"))));
        var preview = await service.PreviewAsync(1, rows);

        var report = await service.ApplyWithReportAsync(1, preview, [], Today);

        Assert.Equal(1, repo.BulkApplyCalls);
        Assert.Equal(CommissionImportService.BulkThreshold, report.Inserted);
        Assert.Equal(CommissionImportService.BulkThreshold, repo.Students.Count);
    }

    [Fact]
    public async Task Given_DecisionsMissing_When_ApplyingWithReport_Then_ItIsRejectedLikeApply()
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(new Section(1, "Mañana"), new Student { Dni = "N0", StudentNumber = "L0", LastName = "A", FirstName = "B", Name = "B A" });
        var service = new CommissionImportService(repo);
        var rows = service.Read(new StringReader(Csv(2, i => ($"N{i}", $"L{i}"))));
        var preview = await service.PreviewAsync(1, rows);

        await Assert.ThrowsAsync<ImportValidationException>(() => service.ApplyWithReportAsync(1, preview, [], Today));
    }
}
