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

    public int AtRiskCount => RiskSummary.High;
    public int TotalStudents => RiskSummary.Total;
}
