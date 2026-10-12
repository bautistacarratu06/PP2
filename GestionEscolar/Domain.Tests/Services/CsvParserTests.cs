namespace Domain.Tests;
using System.Text;

public class CsvParserTests
{
    [Fact]
    public void ParseLine_SimpleLine_ReturnsFields()
    {
        var line = "Juan,20,Buenos Aires";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "20", "Buenos Aires" },
            result
        );
    }

    [Fact]
    public void ParseLine_CommaInsideQuotes_DoesNotSplitField()
    {
        var line = "Juan,\"Buenos Aires, Argentina\",20";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "Buenos Aires, Argentina", "20" },
            result
        );
    }

    [Fact]
    public void ParseLine_EscapedQuotes_ReturnsLiteralQuotes()
    {
        var line = "Juan,\"Dijo \"\"hola\"\"\"";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "Dijo \"hola\"" },
            result
        );
    }

    [Fact]
    public void ParseLine_EmptyFields_PreservesEmptyFields()
    {
        var line = "Juan,,20,";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "", "20", "" },
            result
        );
    }

    [Fact]
    public void ParseLine_EmptyLine_ReturnsOneEmptyField()
    {
        var line = "";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(new[] { "" }, result);
    }

}
