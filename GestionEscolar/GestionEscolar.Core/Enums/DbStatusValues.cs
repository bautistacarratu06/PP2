namespace GestionEscolar.Core.Enums;

// Traduce los enums a los valores exactos que guarda la base (CHECK constraints) y viceversa.
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

    // null o vacío = no hay registro.
    public static AttendanceStatus? ParseAttendanceStatus(string? value) => value switch
    {
        null or "" => null,
        _ when value == AttendanceStatus.Presente.ToDbValue() => AttendanceStatus.Presente,
        _ when value == AttendanceStatus.Ausente.ToDbValue() => AttendanceStatus.Ausente,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Estado de asistencia desconocido.")
    };

    public static SubmissionStatus? ParseSubmissionStatus(string? value) => value switch
    {
        null or "" => null,
        _ when value == SubmissionStatus.Entrego.ToDbValue() => SubmissionStatus.Entrego,
        _ when value == SubmissionStatus.NoEntrego.ToDbValue() => SubmissionStatus.NoEntrego,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Estado de entrega desconocido.")
    };
}
