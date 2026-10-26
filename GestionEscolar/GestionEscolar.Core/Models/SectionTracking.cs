namespace GestionEscolar.Core.Models;

// Seguimiento de una comisión: alumnos ya evaluados y el resumen Alto/Medio/Bajo
// contado sobre esa misma lista.
public sealed record SectionTracking(IReadOnlyList<StudentRiskResult> Students, RiskSummary Summary);
