namespace GestionEscolar.Core.Models;

// Modelo inmutable para el reporte de riesgo de una comisión.
public sealed class SectionRiskReport
{
    public const string NoDataMessage = "Todavía no hay datos para calcular el riesgo";

    public int SectionId { get; init; }
    public string SectionName { get; init; } = string.Empty;
    public RiskSummary RiskSummary { get; init; } = RiskSummary.Empty;
    public bool HasData { get; init; }
    public string StatusMessage { get; init; } = string.Empty;
    public IReadOnlyList<string> Paragraphs { get; init; } = Array.Empty<string>();
    public DateTime GeneratedAt { get; init; }

    // Contactos registrados con alumnos de la comisión (activos o no).
    public int InterventionCount { get; init; }

    // Clases dictadas hasta hoy en la comisión.
    public int ClassCount { get; init; }

    // Entregas hechas y esperadas de los alumnos activos.
    public int SubmittedCount { get; init; }
    public int ExpectedSubmissions { get; init; }

    public int AtRiskCount => RiskSummary.High;
    public int TotalStudents => RiskSummary.Total;
}
