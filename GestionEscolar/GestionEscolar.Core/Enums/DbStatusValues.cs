namespace GestionEscolar.Core.Enums;

// Traduce los enums a los valores exactos que guarda la base (CHECK constraints).
// Así los strings mágicos viven en un solo lugar.
public static class DbStatusValues
{
    public static string ToDbValue(this AttendanceStatus status) => status switch
    {
        AttendanceStatus.Presente => "presente",
        AttendanceStatus.Ausente => "ausente",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static string ToDbValue(this SubmissionStatus status) => status switch
    {
        SubmissionStatus.Entrego => "entregó",
        SubmissionStatus.NoEntrego => "no entregó",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };
}
