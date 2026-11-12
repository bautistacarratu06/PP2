using GestionEscolar.Core.Enums;

namespace UI.Formatting;

// Texto y tipo (success/danger/neutral) de asistencia y entregas: un solo mapa para toda la UI.
public static class StatusLabels
{
    public const string NoRecord = "Sin registro";

    public static IReadOnlyList<string> AttendanceOptions { get; } = [Attendance(AttendanceStatus.Presente), Attendance(AttendanceStatus.Ausente)];

    public static IReadOnlyList<string> SubmissionOptions { get; } = [Submission(SubmissionStatus.Entrego), Submission(SubmissionStatus.NoEntrego)];

    public static string Attendance(AttendanceStatus? status) => status switch
    {
        AttendanceStatus.Presente => "Presente",
        AttendanceStatus.Ausente => "Ausente",
        _ => NoRecord
    };

    public static string Submission(SubmissionStatus? status) => status switch
    {
        SubmissionStatus.Entrego => "Entregó",
        SubmissionStatus.NoEntrego => "No entregó",
        _ => NoRecord
    };

    public static bool TryParseAttendance(string label, out AttendanceStatus status)
    {
        foreach (var candidate in Enum.GetValues<AttendanceStatus>())
        {
            if (Attendance(candidate) == label)
            {
                status = candidate;
                return true;
            }
        }

        status = default;
        return false;
    }

    public static bool TryParseSubmission(string label, out SubmissionStatus status)
    {
        foreach (var candidate in Enum.GetValues<SubmissionStatus>())
        {
            if (Submission(candidate) == label)
            {
                status = candidate;
                return true;
            }
        }

        status = default;
        return false;
    }

    public static string Kind(string? label)
    {
        if (TryParseAttendance(label ?? string.Empty, out var attendance))
            return attendance == AttendanceStatus.Presente ? "success" : "danger";

        if (TryParseSubmission(label ?? string.Empty, out var submission))
            return submission == SubmissionStatus.Entrego ? "success" : "danger";

        return "neutral";
    }
}
