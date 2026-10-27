using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class CommissionImportServiceTests
{
    private static readonly DateTime Today = new(2026, 10, 27);

    private const string CleanCsv =
        """
        DNI,Apellido,Nombre,Legajo,Teléfono,Correo
        30111222,Paz,Ana,12001,1155550101,ana.paz@edu.test
        28999001,Rios,Luis,12002,1155550102,luis.rios@edu.test
        27444555,Sosa,Eva,12003,,eva.sosa@edu.test
        """;

    [Fact]
    public void Given_APath_When_CheckingTheExtension_Then_OnlyCsvIsAccepted()
    {
        Assert.True(CommissionImportService.IsCsvFile("comision.csv"));
        Assert.True(CommissionImportService.IsCsvFile(@"C:\datos\NOMINA.CSV"));
        Assert.False(CommissionImportService.IsCsvFile("comision.txt"));
        Assert.False(CommissionImportService.IsCsvFile("comision.xlsx"));
    }

    [Fact]
    public async Task Given_ACleanFileAndANewName_When_Confirming_Then_TheSectionAndStudentsAreCreated()
    {
        var repo = new FakeCommissionImportRepository();
        var service = new CommissionImportService(repo);
        var rows = service.Read(new StringReader(CleanCsv));

        var preview = await service.PreviewAsync(null, rows);
        var section = await service.CreateAsync("Noche", rows, Today);

        Assert.Equal(3, preview.InsertCount);
        Assert.Empty(preview.Conflicts);
        Assert.Equal("Noche", section.Name);
        var roster = repo.Students.Where(student => student.SectionId == section.Id).ToList();
        Assert.Equal(3, roster.Count);
        Assert.Equal(Today, roster[0].EnrolledOn);
        Assert.Contains(roster, student => student.Dni == "30111222" && student.Phone == "1155550101");
        Assert.Contains(roster, student => student.StudentNumber == "12003" && student.Phone == string.Empty);
    }

    [Theory]
    [InlineData("", "vacío")]
    [InlineData("   ", "vacío")]
    [InlineData("Mañana", "Ya existe")]
    [InlineData("123456789012345678901", "20")]
    public async Task Given_AnInvalidSectionName_When_Confirming_Then_NothingIsWritten(string name, string expected)
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(new Section(1, "Mañana"));
        var service = new CommissionImportService(repo);
        var rows = service.Read(new StringReader(CleanCsv));

        var error = await Assert.ThrowsAsync<ImportValidationException>(() => service.CreateAsync(name, rows, Today));

        Assert.Contains(expected, error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(repo.Sections);
        Assert.Empty(repo.Students);
    }

    [Fact]
    public async Task Given_ARepeatedDniOrAStoredDni_When_CreatingASection_Then_ItIsNotConfirmed()
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(new Section(1, "Mañana"), Student(4, 1, "10", "100", "Alba", "Bea"));
        var service = new CommissionImportService(repo);
        var csv = """
            DNI,Apellido,Nombre,Legajo,Teléfono,Correo
            10,Paz,Ana,200,1,a@edu.test
            20,Rios,Luis,201,2,b@edu.test
            20,Sosa,Eva,202,3,c@edu.test
            """;

        var error = await Assert.ThrowsAsync<ImportValidationException>(() =>
            service.CreateAsync("Noche", service.Read(new StringReader(csv)), Today));

        Assert.Contains("no se confirma", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(repo.Sections);
        Assert.Single(repo.Students);
    }

    [Fact]
    public async Task Given_AnExistingSection_When_Previewing_Then_NewRowsAndConflictsAreSeparated()
    {
        var repo = Seeded();
        var service = new CommissionImportService(repo);

        var preview = await service.PreviewAsync(1, service.Read(new StringReader(MixedCsv)));

        Assert.Equal(2, preview.InsertCount);
        Assert.Equal(4, preview.Conflicts.Count);
        Assert.Contains(preview.ToInsert, row => row.Dni == "40");
        Assert.Contains(preview.ToInsert, row => row.Dni == "70");
        Assert.Equal("DNI y legajo · en la base", Conflict(preview, "1").Explanation);
        Assert.True(Conflict(preview, "1").CanMerge);
        Assert.Equal("DNI · en la base", Conflict(preview, "3").Explanation);
        Assert.False(Conflict(preview, "3").CanMerge);
        Assert.Equal("Legajo · en la base", Conflict(preview, "5").Explanation);
        Assert.False(Conflict(preview, "5").CanMerge);
        Assert.Equal("DNI y legajo · en el archivo", Conflict(preview, "70").Explanation);
        Assert.False(Conflict(preview, "70").CanMerge);
    }

    [Fact]
    public async Task Given_MixedDecisions_When_Confirming_Then_OnlyInsertsAndTheMergeAreWritten()
    {
        var repo = Seeded();
        var service = new CommissionImportService(repo);
        var preview = await service.PreviewAsync(1, service.Read(new StringReader(MixedCsv)));
        var decisions = preview.Conflicts.Select(conflict =>
            conflict.CanMerge ? ImportDecision.Merge : ImportDecision.Omit).ToList();

        await service.ApplyAsync(1, preview, decisions, Today);

        var roster = repo.Students.Where(student => student.SectionId == 1).ToList();
        Assert.Equal(4, roster.Count);
        Assert.Contains(roster, student => student.Dni == "40" && student.StudentNumber == "400");
        Assert.Contains(roster, student => student.Dni == "70" && student.StudentNumber == "700");
        Assert.DoesNotContain(roster, student => student.Dni == "3");
        Assert.DoesNotContain(roster, student => student.Dni == "5");
        Assert.DoesNotContain(roster, student => student.StudentNumber == "701");
        var merged = roster.Single(student => student.Dni == "1");
        Assert.Equal("100", merged.StudentNumber);
        Assert.Equal("1199990000", merged.Phone);
        Assert.Equal("ana.nueva@edu.test", merged.Email);
        Assert.Equal("Paz", merged.LastName);
        Assert.Equal("Ana María", merged.FirstName);
        var other = repo.Students.Single(student => student.Dni == "3");
        Assert.Equal(2, other.SectionId);
        Assert.Equal("viejo", other.Phone);
    }

    [Fact]
    public async Task Given_AMergeThatDoesNotMatchTheSameStudent_When_Confirming_Then_NothingIsWritten()
    {
        var repo = Seeded();
        var service = new CommissionImportService(repo);
        var preview = await service.PreviewAsync(1, service.Read(new StringReader(MixedCsv)));
        var decisions = preview.Conflicts.Select(_ => ImportDecision.Merge).ToList();
        var phones = repo.Students.Select(student => student.Phone).ToList();

        var error = await Assert.ThrowsAsync<ImportValidationException>(() =>
            service.ApplyAsync(1, preview, decisions, Today));

        Assert.Contains("omitir", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(phones, repo.Students.Select(student => student.Phone));
        Assert.Equal(2, repo.Students.Count(student => student.SectionId == 1));
    }

    [Fact]
    public async Task Given_AFileOnlyRepeat_When_Previewing_Then_TheFirstRowCanBeInsertedAndCopiesCannotMerge()
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(new Section(1, "Mañana"));
        var service = new CommissionImportService(repo);
        var csv = """
            DNI,Apellido,Nombre,Legajo,Teléfono,Correo
            70,Costa,Ian,700,1,a@edu.test
            70,Costa,Ian,701,2,b@edu.test
            """;

        var preview = await service.PreviewAsync(1, service.Read(new StringReader(csv)));

        var insert = Assert.Single(preview.ToInsert);
        Assert.Equal("700", insert.StudentNumber);
        var conflict = Assert.Single(preview.Conflicts);
        Assert.False(conflict.CanMerge);
        Assert.Equal(ConflictOrigin.File, conflict.Origin);
        Assert.Contains("DNI", conflict.Explanation);
        Assert.Contains("archivo", conflict.Explanation);
    }

    [Fact]
    public void Given_AFileThatIsNotARosterCsv_When_Reading_Then_ItIsRejected()
    {
        var service = new CommissionImportService(new FakeCommissionImportRepository());

        var error = Assert.Throws<ImportValidationException>(() => service.Read(new StringReader("nombre,edad\nAna,20")));

        Assert.Contains("CSV", error.Message);
    }

    private const string MixedCsv =
        """
        DNI,Apellido,Nombre,Legajo,Teléfono,Correo
        40,Diaz,Leo,400,1155500040,leo@edu.test
        1,Paz,Ana María,100,1199990000,ana.nueva@edu.test
        3,Sosa,Eva,999,1155500003,eva@edu.test
        5,Costa,Ian,200,1155500005,ian@edu.test
        70,Nunez,Sol,700,1155500070,sol@edu.test
        70,Nunez,Sol,700,1155500071,sol2@edu.test
        """;

    private static FakeCommissionImportRepository Seeded()
    {
        var repo = new FakeCommissionImportRepository();
        repo.Seed(
            new Section(1, "Mañana"),
            Student(1, 1, "1", "100", "Paz", "Ana", "viejo"),
            Student(2, 1, "2", "200", "Rios", "Luis", "luis"));
        repo.Seed(new Section(2, "Tarde"), Student(3, 2, "3", "300", "Sosa", "Eva", "viejo"));
        return repo;
    }

    private static ImportConflict Conflict(ImportPreview preview, string dni) =>
        preview.Conflicts.Single(conflict => conflict.Row.Dni == dni);

    private static Student Student(int id, int sectionId, string dni, string number, string last, string first, string phone = "") => new()
    {
        Id = id,
        SectionId = sectionId,
        Dni = dni,
        StudentNumber = number,
        LastName = last,
        FirstName = first,
        Name = $"{first} {last}",
        Phone = phone
    };
}
