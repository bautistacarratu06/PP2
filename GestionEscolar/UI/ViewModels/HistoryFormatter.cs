using System.Globalization;
using GestionEscolar.Core.Models;
using UI.Formatting;

namespace UI.ViewModels;

public sealed record HistoryItem(string Label, string Status, string Kind);

// Texto de cada fila de los historiales del detalle del alumno.
public static class HistoryFormatter
{
    private const string DateFormat = "dd'/'MM'/'yyyy";

    public static HistoryItem Attendance(AttendanceRecord record)
    {
        var status = StatusLabels.Attendance(record.Status);
        return new HistoryItem($"Clase · {Format(record.Date)}", status, StatusLabels.Kind(status));
    }

    public static HistoryItem Submission(Submission submission)
    {
        var status = StatusLabels.Submission(submission.Status);
        return new HistoryItem(
            $"{submission.AssignmentName} · vence {Format(submission.DueDate)}",
            status,
            StatusLabels.Kind(status));
    }

    public static HistoryItem Intervention(Intervention intervention) =>
        new($"{Format(intervention.Date)} · {intervention.Channel} · {intervention.Description}", string.Empty, "neutral");

    private static string Format(DateTime date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);
}
