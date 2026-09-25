namespace Domain.Tests;

public class CSVValidatorTests
{
    [Fact]
    public void IsValid_WhenFileIsNotCsv_ReturnsFalse()
    {
        var validator = new CSVValidator();

        var result = validator.IsValid("file.txt");

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WhenFilePathIsNull_ReturnsFalse()
    {
        var validator = new CSVValidator();

        var result = validator.IsValid(null);

        Assert.False(result);
    }
}
