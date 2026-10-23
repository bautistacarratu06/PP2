using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Entrega de un alumno para un trabajo. Status null = todavía no se cargó.
// StudentName viene como "Apellido, Nombre".
public sealed record SubmissionMark(int StudentId, string StudentName, SubmissionStatus? Status);
