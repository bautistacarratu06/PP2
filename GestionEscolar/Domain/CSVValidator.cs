namespace Domain;

public class CSVValidator
{
    public bool IsValid(string filePath)
    {
        return File.Exists(filePath);
    }
}
