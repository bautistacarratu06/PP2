using System.Reflection;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.Seeding;

// Lee los alumnos del CSV embebido (sample.csv). Hoy solo lo usa Nómina.
public static class StudentSeeder
{
    private const string CsvResourceName = "sample.csv";

    public static IReadOnlyList<Student> GetStudents() =>
        LoadRows().Select(row => row.Student).ToList();

    private static List<StudentTrackingData> LoadRows()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(CsvResourceName)
            ?? throw new FileNotFoundException(
                $"No se encontró el recurso embebido '{CsvResourceName}'.");

        using var reader = new StreamReader(stream);

        return StudentCsvImporter.Import(reader);
    }
}
