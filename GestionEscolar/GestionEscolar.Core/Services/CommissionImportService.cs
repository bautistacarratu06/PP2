using System.Globalization;
using System.Text;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Carga una comisión desde un CSV y, sobre una comisión que ya existe, resuelve cada conflicto.
public sealed class CommissionImportService
{
    private readonly ICommissionImportRepository _repository;

    public CommissionImportService(ICommissionImportRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public static bool IsCsvFile(string? path) =>
        Path.GetExtension(path ?? string.Empty).Equals(".csv", StringComparison.OrdinalIgnoreCase);

    public Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default) =>
        _repository.GetSectionsAsync(cancellationToken);

    public IReadOnlyList<RosterCsvRow> Read(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var headerLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(headerLine))
            throw new ImportValidationException("El CSV está vacío.");

        var headers = CsvParser.ParseLine(headerLine.TrimStart('\uFEFF'))
            .Select(NormalizeHeader)
            .ToList();
        var index = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < headers.Count; i++)
        {
            if (headers[i].Length > 0)
                index[headers[i]] = i;
        }

        foreach (var required in new[] { "dni", "apellido", "nombre", "legajo", "telefono", "correo" })
        {
            if (!index.ContainsKey(required))
                throw new ImportValidationException("El CSV tiene que incluir DNI, apellido, nombre, legajo, teléfono y correo.");
        }

        var rows = new List<RosterCsvRow>();
        var lineNumber = 1;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = CsvParser.ParseLine(line);
            string Field(string key) => index[key] < fields.Count ? fields[index[key]].Trim() : string.Empty;
            var dni = Field("dni");
            var lastName = Field("apellido");
            var firstName = Field("nombre");
            var studentNumber = Field("legajo");
            if (dni.Length == 0 || lastName.Length == 0 || firstName.Length == 0 || studentNumber.Length == 0)
                throw new ImportValidationException($"La fila {lineNumber} tiene DNI, apellido, nombre o legajo vacío.");

            Limit(dni, StudentEntryRules.MaxDniLength, lineNumber, "DNI");
            Limit(lastName, StudentEntryRules.MaxNameLength, lineNumber, "apellido");
            Limit(firstName, StudentEntryRules.MaxNameLength, lineNumber, "nombre");
            Limit(studentNumber, StudentEntryRules.MaxStudentNumberLength, lineNumber, "legajo");
            var phone = Field("telefono");
            var email = Field("correo");
            Limit(phone, StudentEntryRules.MaxPhoneLength, lineNumber, "teléfono");
            Limit(email, StudentEntryRules.MaxEmailLength, lineNumber, "correo");
            rows.Add(new RosterCsvRow(lineNumber, dni, lastName, firstName, studentNumber, phone, email));
        }

        return rows;
    }

    public async Task<ImportPreview> PreviewAsync(int? sectionId, IReadOnlyList<RosterCsvRow> rows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var students = await _repository.GetStudentsAsync(cancellationToken).ConfigureAwait(false);
        return BuildPreview(sectionId, rows, students);
    }

    public async Task<Section> CreateAsync(string? name, IReadOnlyList<RosterCsvRow> rows, DateTime today, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var sections = await _repository.GetSectionsAsync(cancellationToken).ConfigureAwait(false);
        var nameError = StudentEntryRules.SectionNameError(name, sections);
        if (nameError is not null)
            throw new ImportValidationException(nameError);

        var preview = await PreviewAsync(null, rows, cancellationToken).ConfigureAwait(false);
        if (preview.Conflicts.Count > 0)
            throw new ImportValidationException("El archivo tiene DNI o legajos repetidos, o un DNI que ya está guardado. Esta comisión nueva no se confirma.");

        var students = preview.ToInsert.Select(row => ToStudent(row, today)).ToList();
        return await _repository.CreateSectionAsync(name!.Trim(), students, cancellationToken).ConfigureAwait(false);
    }

    public async Task ApplyAsync(
        int sectionId,
        ImportPreview preview,
        IReadOnlyList<ImportDecision> decisions,
        DateTime today,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(decisions);
        if (decisions.Count != preview.Conflicts.Count)
            throw new ImportValidationException("Cada conflicto se resuelve antes de confirmar.");

        for (var i = 0; i < decisions.Count; i++)
        {
            if (decisions[i] == ImportDecision.Merge && !preview.Conflicts[i].CanMerge)
                throw new ImportValidationException("Esa fila solo se puede omitir.");
        }

        var inserts = preview.ToInsert.Select(row => ToStudent(row, today)).ToList();
        var updates = new List<StudentContactChange>();
        for (var i = 0; i < decisions.Count; i++)
        {
            if (decisions[i] != ImportDecision.Merge)
                continue;

            var conflict = preview.Conflicts[i];
            updates.Add(new StudentContactChange(
                conflict.StudentId!.Value,
                conflict.Row.LastName,
                conflict.Row.FirstName,
                conflict.Row.Phone,
                conflict.Row.Email));
        }

        await _repository.ApplyAsync(sectionId, inserts, updates, cancellationToken).ConfigureAwait(false);
    }

    private static ImportPreview BuildPreview(int? sectionId, IReadOnlyList<RosterCsvRow> rows, IReadOnlyList<Student> students)
    {
        var inserts = new List<RosterCsvRow>();
        var conflicts = new List<ImportConflict>();
        var seenDni = new HashSet<string>(StringComparer.Ordinal);
        var seenNumbers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in rows)
        {
            var byDni = students.FirstOrDefault(student => student.Dni == row.Dni);
            var byNumber = sectionId is int section
                ? students.FirstOrDefault(student => student.SectionId == section && student.StudentNumber == row.StudentNumber)
                : null;
            var dniInDb = byDni is not null;
            var numberInDb = byNumber is not null;
            var dniInFile = !seenDni.Add(row.Dni);
            var numberInFile = !seenNumbers.Add(row.StudentNumber);
            var dniClash = dniInDb || dniInFile;
            var numberClash = numberInDb || numberInFile;

            if (!dniClash && !numberClash)
            {
                inserts.Add(row);
                continue;
            }

            var criterion = dniClash && numberClash
                ? ConflictCriterion.Both
                : dniClash ? ConflictCriterion.Dni : ConflictCriterion.StudentNumber;
            var hitsDatabase = dniInDb || numberInDb;
            var hitsFile = dniInFile || numberInFile;
            var origin = hitsDatabase && hitsFile
                ? ConflictOrigin.DatabaseAndFile
                : hitsDatabase ? ConflictOrigin.Database : ConflictOrigin.File;
            var sameStudent = byDni is not null && byNumber is not null && byDni.Id == byNumber.Id;
            conflicts.Add(new ImportConflict
            {
                Row = row,
                Criterion = criterion,
                Origin = origin,
                CanMerge = sameStudent,
                StudentId = sameStudent ? byDni!.Id : null,
                Explanation = Explain(criterion, origin)
            });
        }

        return new ImportPreview(inserts, conflicts);
    }

    private static Student ToStudent(RosterCsvRow row, DateTime today) => new()
    {
        Dni = row.Dni,
        LastName = row.LastName,
        FirstName = row.FirstName,
        Name = $"{row.FirstName} {row.LastName}",
        StudentNumber = row.StudentNumber,
        Phone = row.Phone,
        Email = row.Email,
        EnrolledOn = today.Date
    };

    private static string Explain(ConflictCriterion criterion, ConflictOrigin origin)
    {
        var what = criterion switch
        {
            ConflictCriterion.Dni => "DNI",
            ConflictCriterion.StudentNumber => "Legajo",
            _ => "DNI y legajo"
        };
        var where = origin switch
        {
            ConflictOrigin.Database => "en la base",
            ConflictOrigin.File => "en el archivo",
            _ => "en la base y en el archivo"
        };
        return $"{what} · {where}";
    }

    private static void Limit(string value, int max, int line, string label)
    {
        if (value.Length > max)
            throw new ImportValidationException($"La fila {line} tiene un {label} de más de {max} caracteres.");
    }

    private static string NormalizeHeader(string header)
    {
        var text = header.Trim().TrimStart('\uFEFF').ToLowerInvariant();
        var decomposed = text.Normalize(NormalizationForm.FormD);
        var chars = decomposed.Where(character =>
            CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark);
        return string.Concat(chars).Normalize(NormalizationForm.FormC);
    }
}
