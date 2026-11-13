using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Calendario de una comisión: lista combinada de clases y trabajos.
// La fecha de clase y el nombre del trabajo se validan en CalendarEntryRules antes de guardar.
public class CalendarService : ICalendarService
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
            .Select(s => new CalendarItem(s.Id, CalendarItemKind.Clase, s.Name, s.Date.Date))
            .Concat(assignmentsTask.Result.Select(a => new CalendarItem(a.Id, CalendarItemKind.Trabajo, a.Name, a.DueDate.Date)))
            .OrderBy(i => i.Date)
            .ThenBy(i => i.Kind)
            .ThenBy(i => i.Name, StringComparer.CurrentCulture)
            .ToList();
    }

    public async Task<int> AddClassAsync(int sectionId, DateTime date, string? name = null, CancellationToken cancellationToken = default)
    {
        var sessions = await _repository.GetClassSessionsAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var day = CalendarEntryRules.ClassDay(date, sessions.Select(s => s.Date));

        var trimmed = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        return await _repository.AddClassSessionAsync(sectionId, day, trimmed, cancellationToken).ConfigureAwait(false);
    }

    public Task UpdateItemNameAsync(int id, CalendarItemKind kind, string name, CancellationToken cancellationToken = default)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (kind == CalendarItemKind.Clase)
        {
            return _repository.UpdateClassSessionNameAsync(id, trimmed, cancellationToken);
        }

        return _repository.UpdateAssignmentNameAsync(id, trimmed, cancellationToken);
    }

    public Task<int> AddAssignmentAsync(int sectionId, string? name, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var trimmed = CalendarEntryRules.AssignmentName(name);
        return _repository.AddAssignmentAsync(sectionId, trimmed, CalendarEntryRules.DueDay(dueDate), cancellationToken);
    }
}
