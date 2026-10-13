using System.Globalization;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

public static class StudentCsvImporter
{
    // Header del CSV -> cómo se asigna a StudentTrackingData
    private static readonly Dictionary<string, Action<StudentTrackingData, string>> Setters =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = (s, v) => s.Student.Id = ParseInt(v),
            ["Nombre"] = (s, v) => s.Student.Name = v,
            ["NumeroEstudiante"] = (s, v) => s.Student.StudentNumber = v,
            ["Telefono"] = (s, v) => s.Student.Phone = v,
            ["Correo"] = (s, v) => s.Student.Email = v,
            ["ClasesAsistidas"] = (s, v) => s.AttendedClasses = ParseInt(v),
            ["TareasEntregadas"] = (s, v) => s.SubmittedAssignments = ParseInt(v),
            ["TareasTotales"] = (s, v) => s.TotalAssignments = ParseInt(v),
            ["UltimoContacto"] = (s, v) => s.LastContact = ParseNullableDate(v)
        };

    public static List<StudentTrackingData> Import(TextReader reader)
    {
        var headerLine = reader.ReadLine()
            ?? throw new InvalidDataException("El CSV está vacío.");

        var headers = CsvParser.ParseLine(headerLine)
            .Select(h => h.Trim())
            .ToList();

        var rows = new List<StudentTrackingData>();
        var lineNumber = 1;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = CsvParser.ParseLine(line);
            var row = new StudentTrackingData();

            try
            {
                for (var i = 0; i < headers.Count && i < fields.Count; i++)
                {
                    if (Setters.TryGetValue(headers[i], out var setter))
                        setter(row, fields[i].Trim());
                }
            }
            catch (FormatException ex)
            {
                throw new InvalidDataException($"Dato inválido en la línea {lineNumber}: {ex.Message}", ex);
            }

            rows.Add(row);
        }

        return rows;
    }

    private static int ParseInt(string value) =>
        int.Parse(value, CultureInfo.InvariantCulture);

    private static DateTime? ParseNullableDate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : DateTime.Parse(value, CultureInfo.InvariantCulture);
}
