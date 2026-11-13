using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso CalendarService para inyectarlo en la UI.
public interface ICalendarService
{
    Task<IReadOnlyList<CalendarItem>> GetItemsAsync(int sectionId, CancellationToken cancellationToken = default);
    Task<int> AddClassAsync(int sectionId, DateTime date, string? name = null, CancellationToken cancellationToken = default);
    Task<int> AddAssignmentAsync(int sectionId, string? name, DateTime dueDate, CancellationToken cancellationToken = default);
    Task UpdateItemNameAsync(int id, CalendarItemKind kind, string name, CancellationToken cancellationToken = default);
}
