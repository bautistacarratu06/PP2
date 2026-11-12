using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Compara las filas del CSV contra la base y contra el propio archivo para armar la vista previa.
public static class ImportConflictDetector
{
    public static ImportPreview BuildPreview(int? sectionId, IReadOnlyList<RosterCsvRow> rows, IReadOnlyList<Student> students)
    {
        var inserts = new List<RosterCsvRow>();
        var conflicts = new List<ImportConflict>();
        var seenDni = new HashSet<string>(StringComparer.Ordinal);
        var seenNumbers = new HashSet<string>(StringComparer.Ordinal);
        var lookup = new StudentLookup(students, sectionId);

        foreach (var row in rows)
        {
            var conflict = Detect(row, lookup, seenDni, seenNumbers);
            if (conflict is null)
                inserts.Add(row);
            else
                conflicts.Add(conflict);
        }

        return new ImportPreview(inserts, conflicts);
    }

    private static ImportConflict? Detect(
        RosterCsvRow row,
        StudentLookup lookup,
        HashSet<string> seenDni,
        HashSet<string> seenNumbers)
    {
        var byDni = lookup.ByDni(row.Dni);
        var byNumber = lookup.ByNumber(row.StudentNumber);
        var dniInDb = byDni is not null;
        var numberInDb = byNumber is not null;
        var dniInFile = !seenDni.Add(row.Dni);
        var numberInFile = !seenNumbers.Add(row.StudentNumber);
        var dniClash = dniInDb || dniInFile;
        var numberClash = numberInDb || numberInFile;
        if (!dniClash && !numberClash)
            return null;

        var criterion = dniClash && numberClash
            ? ConflictCriterion.Both
            : dniClash ? ConflictCriterion.Dni : ConflictCriterion.StudentNumber;
        var hitsDatabase = dniInDb || numberInDb;
        var hitsFile = dniInFile || numberInFile;
        var origin = hitsDatabase && hitsFile
            ? ConflictOrigin.DatabaseAndFile
            : hitsDatabase ? ConflictOrigin.Database : ConflictOrigin.File;
        var sameStudent = byDni is not null && byNumber is not null && byDni.Id == byNumber.Id;
        return new ImportConflict
        {
            Row = row,
            Criterion = criterion,
            Origin = origin,
            CanMerge = sameStudent,
            StudentId = sameStudent ? byDni!.Id : null,
            Explanation = Explain(criterion, origin)
        };
    }

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

    // Índices armados una sola vez por vista previa: cada fila se resuelve en O(1).
    // Con duplicados gana el primero, igual que el recorrido lineal anterior.
    private sealed class StudentLookup
    {
        private readonly Dictionary<string, Student> _byDni = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Student> _byNumber = new(StringComparer.Ordinal);

        public StudentLookup(IReadOnlyList<Student> students, int? sectionId)
        {
            foreach (var student in students)
            {
                _byDni.TryAdd(student.Dni, student);
                if (sectionId is int section && student.SectionId == section)
                    _byNumber.TryAdd(student.StudentNumber, student);
            }
        }

        public Student? ByDni(string dni) => _byDni.GetValueOrDefault(dni);

        public Student? ByNumber(string number) => _byNumber.GetValueOrDefault(number);
    }
}
