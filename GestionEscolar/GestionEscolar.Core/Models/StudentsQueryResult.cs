namespace GestionEscolar.Core.Models;

// Resultado de la consulta unificada: filas filtradas/ordenadas + resumen de riesgo solo sobre activos.
public sealed record StudentsQueryResult(
    IReadOnlyList<StudentRiskResult> Students,
    RiskSummary ActiveRiskSummary)
{
    // Filas que cumplen los filtros, antes de recortar la página. Sin paginado es Students.Count.
    public int TotalCount { get; init; }
}
