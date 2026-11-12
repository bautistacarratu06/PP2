using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Carga una comisión desde un CSV y, sobre una comisión que ya existe, resuelve cada conflicto.
public sealed class CommissionImportService : ICommissionImportService
{
    // A partir de esta cantidad de filas nuevas se usa la carga masiva; debajo, el camino normal.
    public const int BulkThreshold = 200;

    private readonly ICommissionImportRepository _repository;

    public CommissionImportService(ICommissionImportRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public static bool IsCsvFile(string? path) => RosterCsvReader.IsCsvFile(path);

    public Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default) =>
        _repository.GetSectionsAsync(cancellationToken);

    public IReadOnlyList<RosterCsvRow> Read(TextReader reader) => RosterCsvReader.Read(reader);

    public async Task<ImportPreview> PreviewAsync(int? sectionId, IReadOnlyList<RosterCsvRow> rows, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var students = await _repository.GetStudentsAsync(cancellationToken).ConfigureAwait(false);
        return ImportConflictDetector.BuildPreview(sectionId, rows, students);
    }

    public async Task<Section> CreateAsync(string? name, IReadOnlyList<RosterCsvRow> rows, DateTime today, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rows);
        var sections = await _repository.GetSectionsAsync(cancellationToken).ConfigureAwait(false);
        var nameError = SectionNameRule.Error(name, sections);
        if (nameError is not null)
            throw new ImportValidationException(nameError);

        var preview = await PreviewAsync(null, rows, cancellationToken).ConfigureAwait(false);
        // Las filas en conflicto (DNI ya guardado o repetido en el archivo) se omiten; el resto se importa.
        if (preview.InsertCount == 0)
            throw new ImportValidationException("Ninguna fila se puede importar: todas tienen un DNI o legajo repetido o ya guardado.");

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
        ValidateDecisions(preview, decisions);

        var inserts = preview.ToInsert.Select(row => ToStudent(row, today)).ToList();
        var updates = BuildUpdates(preview, decisions);
        await _repository.ApplyAsync(sectionId, inserts, updates, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ImportResult> ApplyWithReportAsync(
        int sectionId,
        ImportPreview preview,
        IReadOnlyList<ImportDecision> decisions,
        DateTime today,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(decisions);
        ValidateDecisions(preview, decisions);

        var inserts = preview.ToInsert.Select(row => new NumberedStudent(row.Line, ToStudent(row, today))).ToList();
        var updates = BuildUpdates(preview, decisions);
        return inserts.Count >= BulkThreshold
            ? await _repository.BulkApplyAsync(sectionId, inserts, updates, cancellationToken).ConfigureAwait(false)
            : await _repository.ApplyWithReportAsync(sectionId, inserts, updates, cancellationToken).ConfigureAwait(false);
    }

    private static void ValidateDecisions(ImportPreview preview, IReadOnlyList<ImportDecision> decisions)
    {
        if (decisions.Count != preview.Conflicts.Count)
            throw new ImportValidationException("Cada conflicto se resuelve antes de confirmar.");

        for (var i = 0; i < decisions.Count; i++)
        {
            if (decisions[i] == ImportDecision.Merge && !preview.Conflicts[i].CanMerge)
                throw new ImportValidationException("Esa fila solo se puede omitir.");
        }
    }

    private static List<StudentContactChange> BuildUpdates(ImportPreview preview, IReadOnlyList<ImportDecision> decisions)
    {
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

        return updates;
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
}
