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
            ["Dni"] = (s, v) => s.Student.Dni = v,
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
        var records = new CsvRecordReader(reader);
        var headerLine = records.ReadRecord()
            ?? throw new InvalidDataException("El CSV está vacío.");

        var headers = CsvParser.ParseLine(headerLine)
            .Select(h => h.Trim())
            .ToList();

        var rows = new List<StudentTrackingData>();

        while (records.ReadRecord() is { } record)
        {
            if (string.IsNullOrWhiteSpace(record))
                continue;

            var row = ReadRow(headers, CsvParser.ParseLine(record), records.StartLine);
            ValidateRow(row, headers, records.StartLine);
            rows.Add(row);
        }

        return rows;
    }

    private static StudentTrackingData ReadRow(List<string> headers, List<string> fields, int lineNumber)
    {
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

        return row;
    }

    // Mismas reglas que el importador de nóminas: obligatorios y largos máximos, con el número de fila.
    private static void ValidateRow(StudentTrackingData row, List<string> headers, int line)
    {
        var student = row.Student;

        if (HasColumn(headers, "Dni") && student.Dni.Length == 0)
            throw new InvalidDataException($"La fila {line} tiene el DNI vacío.");

        if (HasColumn(headers, "NumeroEstudiante") && student.StudentNumber.Length == 0)
            throw new InvalidDataException($"La fila {line} tiene el legajo vacío.");

        Limit(student.Dni, StudentEntryRules.MaxDniLength, line, "DNI");
        Limit(student.Name, StudentEntryRules.MaxNameLength, line, "nombre");
        Limit(student.StudentNumber, StudentEntryRules.MaxStudentNumberLength, line, "legajo");
        Limit(student.Phone, StudentEntryRules.MaxPhoneLength, line, "teléfono");
        Limit(student.Email, StudentEntryRules.MaxEmailLength, line, "correo");
    }

    private static bool HasColumn(List<string> headers, string name) =>
        headers.Any(header => header.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static void Limit(string value, int max, int line, string label)
    {
        if (value.Length > max)
            throw new InvalidDataException($"La fila {line} tiene un {label} de más de {max} caracteres.");
    }

    private static int ParseInt(string value) =>
        int.Parse(value, CultureInfo.InvariantCulture);

    private static DateTime? ParseNullableDate(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : DateTime.Parse(value, CultureInfo.InvariantCulture);
}
