using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del servicio de generación de reportes y centralización del resumen de riesgo.
public interface IReportService
{
    Task<SectionRiskReport> GenerateReportAsync(int sectionId, string? sectionName = null, CancellationToken cancellationToken = default);

    Task<RiskSummary> GetActiveRiskSummaryAsync(int sectionId, CancellationToken cancellationToken = default);

    string FormatExportText(SectionRiskReport report);

    string BuildExportFileName(string sectionName, DateTime issuedAt);
}

