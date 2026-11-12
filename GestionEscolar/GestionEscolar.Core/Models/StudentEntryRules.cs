namespace GestionEscolar.Core.Models;

// Reglas de alta de un alumno. El caso de uso las consulta antes de guardar.
public static class StudentEntryRules
{
    public const int MaxSectionNameLength = 20;
    public const int MaxDniLength = 20;
    public const int MaxStudentNumberLength = 20;
    public const int MaxNameLength = 80;
    public const int MaxPhoneLength = 30;
    public const int MaxEmailLength = 120;
    public const int MaxNoteLength = 500;

    public static Student Normalize(StudentDraft draft, DateTime today)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var lastName = Required(draft.LastName, "El apellido no puede estar vacío.");
        var firstName = Required(draft.FirstName, "El nombre no puede estar vacío.");
        var studentNumber = Required(draft.StudentNumber, "El legajo no puede estar vacío.");
        var dni = Required(draft.Dni, "El DNI no puede estar vacío.");
        Limit(lastName, MaxNameLength, "El apellido");
        Limit(firstName, MaxNameLength, "El nombre");
        Limit(studentNumber, MaxStudentNumberLength, "El legajo");
        Limit(dni, MaxDniLength, "El DNI");

        if (draft.EnrolledOn.Date > today.Date)
            throw new StudentEnrollmentException("La fecha de incorporación no puede ser posterior a hoy.");

        return new Student
        {
            LastName = lastName,
            FirstName = firstName,
            Name = $"{firstName} {lastName}",
            StudentNumber = studentNumber,
            Dni = dni,
            EnrolledOn = draft.EnrolledOn.Date,
            Phone = Optional(draft.Phone, MaxPhoneLength, "El teléfono"),
            Email = Optional(draft.Email, MaxEmailLength, "El correo")
        };
    }

    private static string Required(string? value, string message)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new StudentEnrollmentException(message);

        return trimmed;
    }

    private static string Optional(string? value, int max, string label)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        Limit(trimmed, max, label);
        return trimmed;
    }

    private static void Limit(string value, int max, string label)
    {
        if (value.Length > max)
            throw new StudentEnrollmentException($"{label} no puede superar {max} caracteres.");
    }
}
