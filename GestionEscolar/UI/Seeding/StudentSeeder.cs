using System.Globalization;
using System.Reflection;
using System.Text;
using UI.Models;

namespace UI.Seeding;

public static class StudentSeeder
{
    private const string CsvResourceName = "sample.csv";

    // Datos que no vienen del CSV (hardcodeados por ahora)
    private const int TotalClasses = 30;
    private const string DefaultRiskLevel = "Medio";
    private const int DefaultRiskScore = 40;

    public static IReadOnlyList<Student> GetMockedStudentsForRiskGridView() => LoadStudents();

    public static IReadOnlyList<Student> GetMockedStudentsForListGridView() => LoadStudents();

    private static List<Student> LoadStudents()
    {
        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(CsvResourceName)
            ?? throw new FileNotFoundException(
                $"No se encontró el recurso embebido '{CsvResourceName}'. " +
                "Verificá que esté marcado como EmbeddedResource en el .csproj.");

        using var reader = new StreamReader(stream);

        var headers = ParseCsvLine(reader.ReadLine() ?? string.Empty);
        var headerIndexes = headers
            .Select((header, index) => (header, index))
            .ToDictionary(item => item.header, item => item.index, StringComparer.Ordinal);
        var columns = new[]
        {
            (Property: nameof(Student.Id), Header: "Id"),
            (Property: nameof(Student.Name), Header: "Nombre"),
            (Property: nameof(Student.StudentNumber), Header: "NumeroEstudiante"),
            (Property: nameof(Student.Phone), Header: "Telefono"),
            (Property: nameof(Student.Email), Header: "Correo"),
            (Property: nameof(Student.AttendedClasses), Header: "ClasesAsistidas"),
            (Property: nameof(Student.SubmittedAssignments), Header: "TareasEntregadas"),
            (Property: nameof(Student.TotalAssignments), Header: "TareasTotales"),
            (Property: nameof(Student.LastContact), Header: "UltimoContacto")
        };

        var students = new List<Student>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line);
            var student = new Student();
            foreach (var (propertyName, header) in columns)
            {
                if (headerIndexes.TryGetValue(header, out var index) && index < values.Count)
                    SetCsvValue(student, propertyName, values[index]);
            }

            students.Add(student);
        }

        foreach (var student in students)
        {
            student.TotalClasses = TotalClasses;
            student.RiskLevel = DefaultRiskLevel;
            student.RiskScore = DefaultRiskScore;
        }

        return students;
    }

    private static void SetCsvValue(Student student, string propertyName, string value)
    {
        var property = typeof(Student).GetProperty(propertyName)
            ?? throw new InvalidOperationException($"No existe la propiedad '{propertyName}' en Student.");
        var targetType = Nullable.GetUnderlyingType(property.PropertyType);

        if (string.IsNullOrEmpty(value) && targetType is not null)
        {
            property.SetValue(student, null);
            return;
        }

        targetType ??= property.PropertyType;
        var convertedValue = targetType == typeof(string)
            ? value
            : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        property.SetValue(student, convertedValue);
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];
            if (character == '"')
            {
                if (insideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (character == ',' && !insideQuotes)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(character);
            }
        }

        fields.Add(field.ToString());
        return fields;
    }
}
