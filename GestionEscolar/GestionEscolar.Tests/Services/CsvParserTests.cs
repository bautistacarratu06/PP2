using GestionEscolar.Core.Services;

namespace GestionEscolar.Tests.Services;

public class CsvParserTests
{
    [Fact]
    public void Given_SimpleLine_When_Parsing_Then_ReturnsFields()
    {
        var line = "Juan,20,Buenos Aires";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "20", "Buenos Aires" },
            result
        );
    }

    [Fact]
    public void Given_CommaInsideQuotes_When_Parsing_Then_DoesNotSplitField()
    {
        var line = "Juan,\"Buenos Aires, Argentina\",20";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "Buenos Aires, Argentina", "20" },
            result
        );
    }

    [Fact]
    public void Given_EscapedQuotes_When_Parsing_Then_ReturnsLiteralQuotes()
    {
        var line = "Juan,\"Dijo \"\"hola\"\"\"";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "Dijo \"hola\"" },
            result
        );
    }

    [Fact]
    public void Given_EmptyFields_When_Parsing_Then_PreservesEmptyFields()
    {
        var line = "Juan,,20,";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(
            new[] { "Juan", "", "20", "" },
            result
        );
    }

    [Fact]
    public void Given_EmptyLine_When_Parsing_Then_ReturnsOneEmptyField()
    {
        var line = "";

        var result = CsvParser.ParseLine(line);

        Assert.Equal(new[] { "" }, result);
    }

}
