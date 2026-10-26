using System.Globalization;
using GestionEscolar.Core.Services;

namespace GestionEscolar.Tests.Services;

public class StudentCsvImporterTests
{
    [Fact]
    public void Given_KnownHeader_When_Importing_Then_MappedFieldsFillTrackingData()
    {
        const string csv =
            "id,nombre,numeroestudiante,telefono,correo,clasesasistidas,tareasentregadas,tareastotales,ultimocontacto,columnaDesconocida\n" +
            "7,Acosta Ana,LEG-1,555,ana@edu,16,5,10,2026-09-02,ignorar\n";

        var row = Assert.Single(StudentCsvImporter.Import(new StringReader(csv)));

        Assert.Equal(7, row.Student.Id);
        Assert.Equal("Acosta Ana", row.Student.Name);
        Assert.Equal("LEG-1", row.Student.StudentNumber);
        Assert.Equal("555", row.Student.Phone);
        Assert.Equal("ana@edu", row.Student.Email);
        Assert.Equal(16, row.AttendedClasses);
        Assert.Equal(5, row.SubmittedAssignments);
        Assert.Equal(10, row.TotalAssignments);
        Assert.Equal(new DateTime(2026, 9, 2), row.LastContact);
        Assert.Equal(0, row.TotalClasses);
    }

    [Fact]
    public void Given_EmptyFile_When_Importing_Then_Rejected()
    {
        var ex = Assert.Throws<InvalidDataException>(() => StudentCsvImporter.Import(new StringReader(string.Empty)));

        Assert.Contains("vacío", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Given_BlankLine_When_Importing_Then_LineDoesNotCreateRow()
    {
        const string csv = "Nombre\n\nAna\n   \nLuis\n";

        var rows = StudentCsvImporter.Import(new StringReader(csv));

        Assert.Equal(new[] { "Ana", "Luis" }, rows.Select(r => r.Student.Name));
    }

    [Theory]
    [InlineData("Id,Nombre\nabc,Ana\n", 2)]
    [InlineData("Id,UltimoContacto\n1,no-es-fecha\n", 2)]
    [InlineData("Id,UltimoContacto\n\n1,2026-09-02\n2,fecha-mala\n", 4)]
    public void Given_InvalidNumberOrDate_When_Importing_Then_ErrorNamesTheLine(string csv, int line)
    {
        var ex = Assert.Throws<InvalidDataException>(() => StudentCsvImporter.Import(new StringReader(csv)));

        Assert.Contains($"línea {line.ToString(CultureInfo.InvariantCulture)}", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Given_UnknownColumn_When_Importing_Then_Ignored()
    {
        const string csv = "Nombre,Notas\nAna,esto no se mapea\n";

        var row = Assert.Single(StudentCsvImporter.Import(new StringReader(csv)));

        Assert.Equal("Ana", row.Student.Name);
        Assert.Equal(0, row.Student.Id);
    }

    [Fact]
    public void Given_FileWithoutTotalClasses_When_Importing_Then_TotalStaysZero()
    {
        const string csv = "Nombre,ClasesAsistidas\nAna,4\n";

        var row = Assert.Single(StudentCsvImporter.Import(new StringReader(csv)));

        Assert.Equal(4, row.AttendedClasses);
        Assert.Equal(0, row.TotalClasses);
    }
}
