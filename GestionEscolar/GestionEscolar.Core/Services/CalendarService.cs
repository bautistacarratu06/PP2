using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Calendario de una comisión: lista combinada de clases y trabajos.
// La fecha de clase y el nombre del trabajo se validan en CalendarEntryRules antes de guardar.
public class CalendarService
{
    private readonly ICalendarRepository _repository;

    public CalendarService(ICalendarRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    // Ordenado por fecha; en la misma fecha va primero la clase y después los trabajos por nombre.
    public async Task<IReadOnlyList<CalendarItem>> GetItemsAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var sessionsTask = _repository.GetClassSessionsAsync(sectionId, cancellationToken);
        var assignmentsTask = _repository.GetAssignmentsAsync(sectionId, cancellationToken);
        await Task.WhenAll(sessionsTask, assignmentsTask).ConfigureAwait(false);

        return sessionsTask.Result
            .Select(s => new CalendarItem(s.Id, CalendarItemKind.Clase, null, s.Date.Date))
            .Concat(assignmentsTask.Result.Select(a => new CalendarItem(a.Id, CalendarItemKind.Trabajo, a.Name, a.DueDate.Date)))
            .OrderBy(i => i.Date)
            .ThenBy(i => i.Kind)
            .ThenBy(i => i.Name, StringComparer.CurrentCulture)
            .ToList();
    }

    public async Task<int> AddClassAsync(int sectionId, DateTime date, CancellationToken cancellationToken = default)
    {
        var sessions = await _repository.GetClassSessionsAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var day = CalendarEntryRules.ClassDay(date, sessions.Select(s => s.Date));

        return await _repository.AddClassSessionAsync(sectionId, day, cancellationToken).ConfigureAwait(false);
    }

    public Task<int> AddAssignmentAsync(int sectionId, string? name, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var trimmed = CalendarEntryRules.AssignmentName(name);
        return _repository.AddAssignmentAsync(sectionId, trimmed, CalendarEntryRules.DueDay(dueDate), cancellationToken);
    }
}
