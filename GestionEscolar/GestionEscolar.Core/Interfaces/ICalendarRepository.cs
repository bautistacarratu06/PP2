using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Interfaces;

// Clases y trabajos de una comisión.
public interface ICalendarRepository
{
    Task<IReadOnlyList<ClassSession>> GetClassSessionsAsync(int sectionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(int sectionId, CancellationToken cancellationToken = default);

    // Devuelve el id nuevo. Si ya existe una clase en esa fecha lanza CalendarValidationException.
    Task<int> AddClassSessionAsync(int sectionId, DateTime date, CancellationToken cancellationToken = default);

    Task<int> AddAssignmentAsync(int sectionId, string name, DateTime dueDate, CancellationToken cancellationToken = default);
}
