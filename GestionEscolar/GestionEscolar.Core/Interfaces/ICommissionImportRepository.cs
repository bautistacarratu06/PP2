using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Lectura y escritura de una importación de nómina.
public interface ICommissionImportRepository
{
    Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default);

    Task<Section> CreateSectionAsync(string name, IReadOnlyList<Student> students, CancellationToken cancellationToken = default);

    // Todo o nada: si algo falla no queda ninguna fila de la importación.
    Task ApplyAsync(
        int sectionId,
        IReadOnlyList<Student> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default);

    // Inserta en lotes y devuelve qué filas entraron y cuáles se rechazaron, con su línea del CSV.
    Task<ImportResult> ApplyWithReportAsync(
        int sectionId,
        IReadOnlyList<NumberedStudent> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default);

    // Camino para importaciones grandes: carga masiva y conflictos resueltos en la base.
    Task<ImportResult> BulkApplyAsync(
        int sectionId,
        IReadOnlyList<NumberedStudent> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default);
}
