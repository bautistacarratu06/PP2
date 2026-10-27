using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

public sealed record RosterCsvRow(
    int Line,
    string Dni,
    string LastName,
    string FirstName,
    string StudentNumber,
    string Phone,
    string Email);

public sealed record StudentContactChange(int StudentId, string LastName, string FirstName, string Phone, string Email);

public sealed class ImportConflict
{
    public required RosterCsvRow Row { get; init; }
    public ConflictCriterion Criterion { get; init; }
    public ConflictOrigin Origin { get; init; }
    public bool CanMerge { get; init; }
    public int? StudentId { get; init; }
    public string Explanation { get; init; } = string.Empty;
}

public sealed class ImportPreview
{
    public ImportPreview(IReadOnlyList<RosterCsvRow> toInsert, IReadOnlyList<ImportConflict> conflicts)
    {
        ToInsert = toInsert;
        Conflicts = conflicts;
    }

    public IReadOnlyList<RosterCsvRow> ToInsert { get; }
    public IReadOnlyList<ImportConflict> Conflicts { get; }
    public int InsertCount => ToInsert.Count;
}
