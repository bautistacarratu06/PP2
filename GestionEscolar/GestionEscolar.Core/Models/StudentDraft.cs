namespace GestionEscolar.Core.Models;

// Datos que el docente escribe para dar de alta un alumno en una comisión.
public sealed record StudentDraft(
    string? LastName,
    string? FirstName,
    string? StudentNumber,
    string? Dni,
    DateTime EnrolledOn,
    string? Phone = null,
    string? Email = null);
