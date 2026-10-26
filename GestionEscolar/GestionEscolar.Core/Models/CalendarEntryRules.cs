namespace GestionEscolar.Core.Models;

// Reglas de alta del calendario. El caso de uso las consulta antes de guardar.
// La UNIQUE de la base queda como red de seguridad.
public static class CalendarEntryRules
{
    public const int MaxAssignmentNameLength = 120; // dbo.assignment.name NVARCHAR(120)

    public static DateTime ClassDay(DateTime date, IEnumerable<DateTime> existingDaysInSection)
    {
        ArgumentNullException.ThrowIfNull(existingDaysInSection);

        var day = date.Date;
        if (existingDaysInSection.Any(existing => existing.Date == day))
            throw new CalendarValidationException($"Ya hay una clase el {day:dd/MM/yyyy} en esta comisión.");

        return day;
    }

    public static string AssignmentName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new CalendarValidationException("El trabajo necesita un nombre.");

        if (trimmed.Length > MaxAssignmentNameLength)
            throw new CalendarValidationException($"El nombre no puede superar {MaxAssignmentNameLength} caracteres.");

        return trimmed;
    }

    public static DateTime DueDay(DateTime dueDate) => dueDate.Date;
}
