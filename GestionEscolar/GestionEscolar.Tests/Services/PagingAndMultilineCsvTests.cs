using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class PagingAndMultilineCsvTests
{
    private static StudentTrackingData Row(int i, bool active = true) => new()
    {
        Student = new Student
        {
            LastName = $"Apellido{i:D4}",
            FirstName = "Nombre",
            Name = $"Nombre Apellido{i:D4}",
            StudentNumber = i.ToString(),
            IsActive = active
        },
        AttendedClasses = i % 9,
        TotalClasses = 8,
        SubmittedAssignments = 0,
        TotalAssignments = 4
    };

    private static StudentsQueryService ServiceWith(int count)
    {
        var repo = new FakeTrackingRepository().WithSection(1, Enumerable.Range(0, count).Select(i => Row(i)).ToArray());
        return new StudentsQueryService(repo, new RiskConfig());
    }

    [Fact]
    public async Task Given_1000Students_When_AskingForPage2OfSize50_Then_Rows51To100AndWholeSummary()
    {
        var service = ServiceWith(1000);

        var all = await service.QueryAsync(1);
        var page = await service.QueryAsync(1, new StudentsQueryFilter { Page = 2, PageSize = 50 });

        Assert.Equal(50, page.Students.Count);
        Assert.Equal(
            all.Students.Skip(50).Take(50).Select(s => s.Data.Student.StudentNumber),
            page.Students.Select(s => s.Data.Student.StudentNumber));
        Assert.Equal(all.ActiveRiskSummary, page.ActiveRiskSummary);
        Assert.Equal(1000, page.ActiveRiskSummary.Total);
    }

    [Fact]
    public async Task Given_NoPageSize_When_Querying_Then_AllRowsAreReturned()
    {
        var service = ServiceWith(120);

        var result = await service.QueryAsync(1, new StudentsQueryFilter { Page = 3 });

        Assert.Equal(120, result.Students.Count);
    }

    [Fact]
    public async Task Given_APageBeyondTheEnd_When_Querying_Then_NoRowsButSummaryStays()
    {
        var service = ServiceWith(30);

        var result = await service.QueryAsync(1, new StudentsQueryFilter { Page = 5, PageSize = 20 });

        Assert.Empty(result.Students);
        Assert.Equal(30, result.ActiveRiskSummary.Total);
    }

    [Fact]
    public void Given_AQuotedFieldWithALineBreak_When_Reading_Then_ItIsOneRecordAndLinesStayCounted()
    {
        const string csv =
            "DNI,Apellido,Nombre,Legajo,Teléfono,Correo\n" +
            "1,\"Paz\nde la Vega\",Ana,L1,555,a@edu\n" +
            "2,Rios,Luis,L2,556,l@edu\n";
        var service = new CommissionImportService(new FakeCommissionImportRepository());

        var rows = service.Read(new StringReader(csv));

        Assert.Equal(2, rows.Count);
        Assert.Equal("Paz\nde la Vega", rows[0].LastName);
        Assert.Equal(2, rows[0].Line);
        Assert.Equal("Rios", rows[1].LastName);
        Assert.Equal(4, rows[1].Line);
    }

    [Fact]
    public void Given_EscapedQuotesAcrossLines_When_ReadingRecords_Then_ParityKeepsTheRecordTogether()
    {
        var reader = new CsvRecordReader(new StringReader("a,\"dijo \"\"hola\"\"\nchau\",c\nx,y,z"));

        var first = reader.ReadRecord();
        var second = reader.ReadRecord();

        Assert.Equal(new[] { "a", "dijo \"hola\"\nchau", "c" }, CsvParser.ParseLine(first!));
        Assert.Equal("x,y,z", second);
        Assert.Equal(3, reader.StartLine);
        Assert.Null(reader.ReadRecord());
    }

    [Fact]
    public void Given_AnOversizedDniInTheTrackingImporter_When_Importing_Then_TheLineIsNamed()
    {
        var csv = $"Dni,Nombre\n1,Ana\n{new string('9', StudentEntryRules.MaxDniLength + 1)},Luis\n";

        var ex = Assert.Throws<InvalidDataException>(() => StudentCsvImporter.Import(new StringReader(csv)));

        Assert.Contains("fila 3", ex.Message, StringComparison.Ordinal);
        Assert.Contains("DNI", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Given_AnEmptyDniColumnValue_When_Importing_Then_ItIsRejectedWithTheLine()
    {
        var ex = Assert.Throws<InvalidDataException>(() =>
            StudentCsvImporter.Import(new StringReader("Dni,Nombre\n,Ana\n")));

        Assert.Contains("fila 2", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Given_AMultilineQuotedName_When_ImportingTracking_Then_ItIsOneRow()
    {
        var rows = StudentCsvImporter.Import(new StringReader("Nombre,Telefono\n\"Ana\nPaz\",555\nLuis,556\n"));

        Assert.Equal(2, rows.Count);
        Assert.Equal("Ana\nPaz", rows[0].Student.Name);
    }
}
