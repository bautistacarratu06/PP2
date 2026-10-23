using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Asistencia de un alumno a una clase. Status null = todavía no se cargó.
// StudentName viene como "Apellido, Nombre".
public sealed record AttendanceMark(int StudentId, string StudentName, AttendanceStatus? Status);
