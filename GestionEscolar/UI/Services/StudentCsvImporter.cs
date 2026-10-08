using System.Globalization;
using UI.Models;

namespace UI.Services;

public static class StudentCsvImporter
{
    // Header del CSV -> cómo se asigna al Student
    private static readonly Dictionary<string, Action<Student, string>> Setters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = (s, v) => s.Id = ParseInt(v),
            ["Nombre"] = (s, v) => s.Name = v,
            ["NumeroEstudiante"] = (s, v) => s.StudentNumber = v,
            ["Telefono"] = (s, v) => s.Phone = v,
            ["Correo"] = (s, v) => s.Email = v,
            ["ClasesAsistidas"] = (s, v) => s.AttendedClasses = ParseInt(v),
            ["TareasEntregadas"] = (s, v) => s.SubmittedAssignments = ParseInt(v),
            ["TareasTotales"] = (s, v) => s.TotalAssignments = ParseInt(v),
            ["UltimoContacto"] = (s, v) => s.LastContact = ParseNullableDate(v)
        };

    public static List<Student> Import(TextReader reader)
    {
        var headerLine = reader.ReadLine()
            ?? throw new InvalidDataException("El CSV está vacío.");

        var headers = CsvParser.ParseLine(headerLine)
            .Select(h => h.Trim())
            .ToList();

        var students = new List<Student>();
        var lineNumber = 1;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = CsvParser.ParseLine(line);
            var student = new Student();

            try
            {
                for (var i = 0; i < headers.Count && i < fields.Count; i++)
                {
                    if (Setters.TryGetValue(headers[i], out var setter))
                        setter(student, fields[i].Trim());
                }
            }
            catch (FormatException ex)
            {
                throw new InvalidDataException($"Dato inválido en la línea {lineNumber}: {ex.Message}", ex);
            }

            students.Add(student);
        }

        return students;
    }

    private static int ParseInt(string value) =>
        int.Parse(value, CultureInfo.InvariantCulture);

    private static DateTime? ParseNullableDate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : DateTime.Parse(value, CultureInfo.InvariantCulture);
}