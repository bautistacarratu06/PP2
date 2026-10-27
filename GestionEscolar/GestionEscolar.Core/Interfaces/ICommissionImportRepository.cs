using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Lectura y escritura de una importación de nómina, en una sola operación de guardado.
public interface ICommissionImportRepository
{
    Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default);

    Task<Section> CreateSectionAsync(string name, IReadOnlyList<Student> students, CancellationToken cancellationToken = default);

    Task ApplyAsync(
        int sectionId,
        IReadOnlyList<Student> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default);
}