using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso CommissionImportService para inyectarlo en la UI.
public interface ICommissionImportService
{
    Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default);
    IReadOnlyList<RosterCsvRow> Read(TextReader reader);
    Task<ImportPreview> PreviewAsync(int? sectionId, IReadOnlyList<RosterCsvRow> rows, CancellationToken cancellationToken = default);
    Task<Section> CreateAsync(string? name, IReadOnlyList<RosterCsvRow> rows, DateTime today, CancellationToken cancellationToken = default);
    Task ApplyAsync(int sectionId, ImportPreview preview, IReadOnlyList<ImportDecision> decisions, DateTime today, CancellationToken cancellationToken = default);

    Task<ImportResult> ApplyWithReportAsync(int sectionId, ImportPreview preview, IReadOnlyList<ImportDecision> decisions, DateTime today, CancellationToken cancellationToken = default);
}
