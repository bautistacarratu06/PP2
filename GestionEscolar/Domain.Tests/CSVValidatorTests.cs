namespace Domain.Tests;

public class CSVValidatorTests
{
    [Fact]
    public void IsValid_WhenFileExists_ReturnsTrue()
    {
        var validator = new CSVValidator();
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "alumnos.csv");

        var result = validator.IsValid(path);

        Assert.True(result);
    }

    [Fact]
    public void IsValid_WhenTempCsvExists_ReturnsTrue()
    {
        var validator = new CSVValidator();
        var path = Path.Combine(Path.GetTempPath(), $"alumnos-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, "nombre,apellido\n");

        try
        {
            var result = validator.IsValid(path);

            Assert.True(result);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
