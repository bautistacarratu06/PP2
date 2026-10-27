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

    public static string ToDbValue(this ContactChannel channel) => channel switch
    {
        ContactChannel.Llamada => "Llamada",
        ContactChannel.WhatsApp => "WhatsApp",
        ContactChannel.Correo => "Correo",
        ContactChannel.Presencial => "Presencial",
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, null)
    };

    public static string ToDbValue(this ContactOutcome outcome) => outcome switch
    {
        ContactOutcome.ComprometioVolver => "Contactado - se comprometió a volver",
        ContactOutcome.SinNovedades => "Contactado - sin novedades",
        ContactOutcome.NoContesto => "No contestó",
        ContactOutcome.AbandonoConfirmado => "Abandonó confirmado",
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
    };

    public static ContactChannel? ParseContactChannel(string? value) => value switch
    {
        null or "" => null,
        _ when value == ContactChannel.Llamada.ToDbValue() => ContactChannel.Llamada,
        _ when value == ContactChannel.WhatsApp.ToDbValue() => ContactChannel.WhatsApp,
        _ when value == ContactChannel.Correo.ToDbValue() => ContactChannel.Correo,
        _ when value == ContactChannel.Presencial.ToDbValue() => ContactChannel.Presencial,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Medio de contacto desconocido.")
    };

    public static ContactOutcome? ParseContactOutcome(string? value) => value switch
    {
        null or "" => null,
        _ when value == ContactOutcome.ComprometioVolver.ToDbValue() => ContactOutcome.ComprometioVolver,
        _ when value == ContactOutcome.SinNovedades.ToDbValue() => ContactOutcome.SinNovedades,
        _ when value == ContactOutcome.NoContesto.ToDbValue() => ContactOutcome.NoContesto,
        _ when value == ContactOutcome.AbandonoConfirmado.ToDbValue() => ContactOutcome.AbandonoConfirmado,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Resultado de contacto desconocido.")
    };
}
