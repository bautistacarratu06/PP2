namespace GestionEscolar.Core.Enums;

// Traduce los enums a los valores exactos que guarda la base (CHECK constraints) y viceversa.
// Así los strings mágicos viven en un solo lugar: una lista (valor, texto) por enum.
public static class DbStatusValues
{
    private static readonly (AttendanceStatus Value, string Db)[] AttendanceMap =
    {
        (AttendanceStatus.Presente, "presente"),
        (AttendanceStatus.Ausente, "ausente")
    };

    private static readonly (SubmissionStatus Value, string Db)[] SubmissionMap =
    {
        (SubmissionStatus.Entrego, "entregó"),
        (SubmissionStatus.NoEntrego, "no entregó")
    };

    private static readonly (ContactChannel Value, string Db)[] ChannelMap =
    {
        (ContactChannel.Llamada, "Llamada"),
        (ContactChannel.WhatsApp, "WhatsApp"),
        (ContactChannel.Correo, "Correo"),
        (ContactChannel.Presencial, "Presencial")
    };

    private static readonly (ContactOutcome Value, string Db)[] OutcomeMap =
    {
        (ContactOutcome.ComprometioVolver, "Contactado - se comprometió a volver"),
        (ContactOutcome.SinNovedades, "Contactado - sin novedades"),
        (ContactOutcome.NoContesto, "No contestó"),
        (ContactOutcome.AbandonoConfirmado, "Abandonó confirmado")
    };

    public static string ToDbValue(this AttendanceStatus status) => ToDb(AttendanceMap, status, nameof(status));

    public static string ToDbValue(this SubmissionStatus status) => ToDb(SubmissionMap, status, nameof(status));

    public static string ToDbValue(this ContactChannel channel) => ToDb(ChannelMap, channel, nameof(channel));

    public static string ToDbValue(this ContactOutcome outcome) => ToDb(OutcomeMap, outcome, nameof(outcome));

    // null o vacío = no hay registro.
    public static AttendanceStatus? ParseAttendanceStatus(string? value) =>
        FromDb(AttendanceMap, value, "Estado de asistencia desconocido.");

    public static SubmissionStatus? ParseSubmissionStatus(string? value) =>
        FromDb(SubmissionMap, value, "Estado de entrega desconocido.");

    public static ContactChannel? ParseContactChannel(string? value) =>
        FromDb(ChannelMap, value, "Medio de contacto desconocido.");

    public static ContactOutcome? ParseContactOutcome(string? value) =>
        FromDb(OutcomeMap, value, "Resultado de contacto desconocido.");

    private static string ToDb<T>((T Value, string Db)[] map, T value, string paramName) where T : struct, Enum
    {
        foreach (var pair in map)
        {
            if (EqualityComparer<T>.Default.Equals(pair.Value, value))
                return pair.Db;
        }

        throw new ArgumentOutOfRangeException(paramName, value, null);
    }

    private static T? FromDb<T>((T Value, string Db)[] map, string? value, string message) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return null;

        foreach (var pair in map)
        {
            if (pair.Db == value)
                return pair.Value;
        }

        throw new ArgumentOutOfRangeException(nameof(value), value, message);
    }
}
