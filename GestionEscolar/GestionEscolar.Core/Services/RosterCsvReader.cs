using System.Globalization;
using System.Text;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Lee el CSV de una nómina: detecta el archivo, normaliza los encabezados y valida cada fila.
public static class RosterCsvReader
{
    private static readonly string[] RequiredHeaders = { "dni", "apellido", "nombre", "legajo", "telefono", "correo" };

    public static bool IsCsvFile(string? path) =>
        Path.GetExtension(path ?? string.Empty).Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<RosterCsvRow> Read(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var records = new CsvRecordReader(reader);
        var index = ReadHeaderIndex(records);
        var rows = new List<RosterCsvRow>();
        while (records.ReadRecord() is { } record)
        {
            if (string.IsNullOrWhiteSpace(record))
                continue;

            rows.Add(ParseRow(record, records.StartLine, index));
        }

        return rows;
    }

    private static Dictionary<string, int> ReadHeaderIndex(CsvRecordReader reader)
    {
        var headerLine = reader.ReadRecord();
        if (string.IsNullOrWhiteSpace(headerLine))
            throw new ImportValidationException("El CSV está vacío.");

        var headers = CsvParser.ParseLine(headerLine.TrimStart('﻿')).Select(NormalizeHeader).ToList();
        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < headers.Count; i++)
        {
            if (headers[i].Length > 0)
                index[headers[i]] = i;
        }

        if (RequiredHeaders.Any(required => !index.ContainsKey(required)))
            throw new ImportValidationException("El CSV tiene que incluir DNI, apellido, nombre, legajo, teléfono y correo.");

        return index;
    }

    private static RosterCsvRow ParseRow(string line, int lineNumber, Dictionary<string, int> index)
    {
        var fields = CsvParser.ParseLine(line);
        string Field(string key) => index[key] < fields.Count ? fields[index[key]].Trim() : string.Empty;
        var dni = Field("dni");
        var lastName = Field("apellido");
        var firstName = Field("nombre");
        var studentNumber = Field("legajo");
        if (dni.Length == 0 || lastName.Length == 0 || firstName.Length == 0 || studentNumber.Length == 0)
            throw new ImportValidationException($"La fila {lineNumber} tiene DNI, apellido, nombre o legajo vacío.");

        var phone = Field("telefono");
        var email = Field("correo");
        Limit(dni, StudentEntryRules.MaxDniLength, lineNumber, "DNI");
        Limit(lastName, StudentEntryRules.MaxNameLength, lineNumber, "apellido");
        Limit(firstName, StudentEntryRules.MaxNameLength, lineNumber, "nombre");
        Limit(studentNumber, StudentEntryRules.MaxStudentNumberLength, lineNumber, "legajo");
        Limit(phone, StudentEntryRules.MaxPhoneLength, lineNumber, "teléfono");
        Limit(email, StudentEntryRules.MaxEmailLength, lineNumber, "correo");
        return new RosterCsvRow(lineNumber, dni, lastName, firstName, studentNumber, phone, email);
    }

    private static void Limit(string value, int max, int line, string label)
    {
        if (value.Length > max)
            throw new ImportValidationException($"La fila {line} tiene un {label} de más de {max} caracteres.");
    }

    private static string NormalizeHeader(string header)
    {
        var text = header.Trim().TrimStart('﻿').ToLowerInvariant();
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var chars = decomposed.Where(character =>
            CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark);
        return string.Concat(chars).Normalize(NormalizationForm.FormC);
    }
}
