using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Calendario de una comisión: lista combinada de clases y trabajos y alta con validación.
public class CalendarService
{
    public const int MaxAssignmentNameLength = 120; // dbo.assignment.name NVARCHAR(120)

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
        var day = date.Date;
        var sessions = await _repository.GetClassSessionsAsync(sectionId, cancellationToken).ConfigureAwait(false);
        if (sessions.Any(s => s.Date.Date == day))
            throw new CalendarValidationException($"Ya hay una clase el {day:dd/MM/yyyy} en esta comisión.");

        return await _repository.AddClassSessionAsync(sectionId, day, cancellationToken).ConfigureAwait(false);
    }

    public Task<int> AddAssignmentAsync(int sectionId, string? name, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new CalendarValidationException("El trabajo necesita un nombre.");
        if (trimmed.Length > MaxAssignmentNameLength)
            throw new CalendarValidationException($"El nombre no puede superar {MaxAssignmentNameLength} caracteres.");

        return _repository.AddAssignmentAsync(sectionId, trimmed, dueDate.Date, cancellationToken);
    }
}
