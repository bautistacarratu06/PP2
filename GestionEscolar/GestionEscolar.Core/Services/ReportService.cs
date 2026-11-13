using System.Globalization;
using System.Text;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Caso de uso para la emisión del reporte de riesgo y la consulta centralizada del resumen de riesgo activo.
public class ReportService : IReportService
{
    private readonly ITrackingRepository _repository;
    private readonly RiskConfig _config;

    public ReportService(ITrackingRepository repository, RiskConfig config)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<RiskSummary> GetActiveRiskSummaryAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.GetRowsBySectionAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var activeRows = rows.Where(r => r.Student.IsActive).ToList();
        if (activeRows.Count == 0)
            return RiskSummary.Empty;

        var evaluated = activeRows.Select(Evaluate).ToList();
        return RiskSummary.FromResults(evaluated);
    }

    public async Task<SectionRiskReport> GenerateReportAsync(
        int sectionId,
        string? sectionName = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository.GetRowsBySectionAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var resolvedName = string.IsNullOrWhiteSpace(sectionName) ? $"Comisión #{sectionId}" : sectionName.Trim();
        var activeRows = rows.Where(r => r.Student.IsActive).ToList();
        var interventions = rows.Sum(r => r.ContactCount);
        // Cada alumno cuenta las clases desde su inscripción; el máximo son las de la comisión.
        var classes = rows.Count == 0 ? 0 : rows.Max(r => r.TotalClasses);
        var submitted = activeRows.Sum(r => r.SubmittedAssignments);
        var expected = activeRows.Sum(r => r.TotalAssignments);

        if (activeRows.Count == 0)
        {
            return new SectionRiskReport
            {
                SectionId = sectionId,
                SectionName = resolvedName,
                RiskSummary = RiskSummary.Empty,
                HasData = false,
                StatusMessage = "La comisión no tiene estudiantes activos cargados.",
                Paragraphs = new[]
                {
                    "No se encontraron estudiantes activos cargados en la comisión para emitir el reporte."
                },
                GeneratedAt = DateTime.Now,
                InterventionCount = interventions,
                ClassCount = classes,
                SubmittedCount = submitted,
                ExpectedSubmissions = expected
            };
        }

        // CA3: Si una comisión tiene estudiantes pero cero asistencias y entregas registradas en su historial,
        // no se calcula riesgo ni se muestra "0 en riesgo".
        var hasHistory = activeRows.Any(r => r.TotalClasses > 0 || r.TotalAssignments > 0);
        if (!hasHistory)
        {
            return new SectionRiskReport
            {
                SectionId = sectionId,
                SectionName = resolvedName,
                RiskSummary = RiskSummary.Empty,
                HasData = false,
                StatusMessage = SectionRiskReport.NoDataMessage,
                Paragraphs = new[]
                {
                    SectionRiskReport.NoDataMessage,
                    "La comisión cuenta con estudiantes matriculados, pero aún no se han registrado clases dictadas ni entregas de trabajos prácticos que permitan realizar el cálculo de riesgo."
                },
                GeneratedAt = DateTime.Now,
                InterventionCount = interventions,
                ClassCount = classes,
                SubmittedCount = submitted,
                ExpectedSubmissions = expected
            };
        }

        var evaluated = activeRows.Select(Evaluate).ToList();
        var summary = RiskSummary.FromResults(evaluated);

        var total = summary.Total;
        var high = summary.High;
        var medium = summary.Medium;
        var low = summary.Low;

        var avgAttendance = activeRows.Count > 0
            ? (int)Math.Round(activeRows.Average(r => r.AttendancePercent), MidpointRounding.AwayFromZero)
            : 0;
        var avgDelivery = activeRows.Count > 0
            ? (int)Math.Round(activeRows.Average(r => r.DeliveryPercent), MidpointRounding.AwayFromZero)
            : 0;

        var atRiskSentence = FormatAtRiskSentence(high, total);

        var paragraph1 = $"{atRiskSentence} El diagnóstico actual identifica {high} casos en riesgo alto, {medium} en riesgo medio y {low} en riesgo bajo sobre un total de {total} estudiantes activos.";
        var paragraph2 = $"En cuanto al desempeño general, la comisión registra una asistencia promedio del {avgAttendance}% y una tasa de entrega de trabajos del {avgDelivery}%.";

        string paragraph3;
        if (high > 0)
        {
            paragraph3 = "Se recomienda coordinar instancias de tutoría y seguimiento personalizado con los estudiantes en riesgo alto para prevenir el abandono de la materia.";
        }
        else if (medium > 0)
        {
            paragraph3 = "No se observan casos en riesgo alto, aunque se aconseja monitorear a los alumnos en riesgo medio para evitar que desmejoren su regularidad.";
        }
        else
        {
            paragraph3 = "La comisión presenta una cursada regular y sin alertas críticas de deserción registradas a la fecha.";
        }

        return new SectionRiskReport
        {
            SectionId = sectionId,
            SectionName = resolvedName,
            RiskSummary = summary,
            HasData = true,
            StatusMessage = string.Empty,
            Paragraphs = new[] { paragraph1, paragraph2, paragraph3 },
            GeneratedAt = DateTime.Now,
            InterventionCount = interventions,
            ClassCount = classes,
            SubmittedCount = submitted,
            ExpectedSubmissions = expected
        };
    }

    public string FormatExportText(SectionRiskReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var sb = new StringBuilder();
        sb.AppendLine("GESTIÓN ESCOLAR");
        sb.AppendLine("Reporte de riesgo por comisión");
        sb.AppendLine(new string('=', 40));
        sb.AppendLine();
        sb.AppendLine($"Comisión: {report.SectionName}");
        sb.AppendLine($"Emitido: {report.GeneratedAt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
        sb.AppendLine();
        sb.AppendLine("Resumen");
        sb.AppendLine(new string('-', 7));

        if (!report.HasData)
        {
            var headline = string.IsNullOrWhiteSpace(report.StatusMessage)
                ? SectionRiskReport.NoDataMessage
                : report.StatusMessage;
            sb.AppendLine(headline);
        }
        else
        {
            sb.AppendLine(FormatAtRiskSentence(report.AtRiskCount, report.TotalStudents));
        }

        sb.AppendLine();
        foreach (var paragraph in report.Paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
                continue;
            sb.AppendLine(paragraph);
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd() + Environment.NewLine;
    }

    public string BuildExportFileName(string sectionName, DateTime issuedAt)
    {
        var safe = SanitizeFileToken(sectionName);
        if (string.IsNullOrWhiteSpace(safe))
            safe = "SinNombre";

        var stamp = issuedAt.ToString("yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
        return $"Reporte_Comision_{safe}_{stamp}.txt";
    }

    private static string FormatAtRiskSentence(int high, int total) =>
        high == 1
            ? $"1 de {total} estudiantes está en riesgo de abandonar."
            : $"{high} de {total} estudiantes están en riesgo de abandonar.";

    private static string SanitizeFileToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value.Normalize(NormalizationForm.FormD);
        var buffer = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(ch))
                buffer.Append(ch);
            else
                buffer.Append('_');
        }

        var collapsed = buffer.ToString();
        while (collapsed.Contains("__", StringComparison.Ordinal))
            collapsed = collapsed.Replace("__", "_", StringComparison.Ordinal);

        return collapsed.Trim('_');
    }

    private StudentRiskResult Evaluate(StudentTrackingData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var attended = data.TotalClasses < 0
            ? data.AttendedClasses
            : Math.Clamp(data.AttendedClasses, 0, data.TotalClasses);
        var submitted = data.TotalAssignments < 0
            ? data.SubmittedAssignments
            : Math.Clamp(data.SubmittedAssignments, 0, data.TotalAssignments);

        var evaluation = RiskRule.Evaluate(
            attended,
            data.TotalClasses,
            submitted,
            data.TotalAssignments,
            _config);
        return new StudentRiskResult(data, evaluation.Score, evaluation.Level);
    }
}

