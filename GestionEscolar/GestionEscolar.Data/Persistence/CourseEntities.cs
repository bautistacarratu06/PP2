namespace GestionEscolar.Data.Persistence;

// Filas de course_tracking. No son los modelos de Core: el repositorio los traduce.
// La clave de student es dni; id es identity y es lo que apuntan las claves foráneas.

public sealed class SectionEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<StudentEntity> Students { get; set; } = [];

    public ICollection<ClassSessionEntity> ClassSessions { get; set; } = [];

    public ICollection<AssignmentEntity> Assignments { get; set; } = [];
}

public sealed class StudentEntity
{
    public int Id { get; set; }

    public string Dni { get; set; } = string.Empty;

    public int SectionId { get; set; }

    public string StudentNumber { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateTime EnrolledOn { get; set; }

    public DateTime CreatedAt { get; set; }

    public SectionEntity Section { get; set; } = null!;

    public ICollection<AttendanceRecordEntity> AttendanceRecords { get; set; } = [];

    public ICollection<SubmissionEntity> Submissions { get; set; } = [];

    public ICollection<InterventionEntity> Interventions { get; set; } = [];
}

public sealed class ClassSessionEntity
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public DateTime SessionDate { get; set; }

    public SectionEntity Section { get; set; } = null!;

    public ICollection<AttendanceRecordEntity> AttendanceRecords { get; set; } = [];
}

public sealed class AssignmentEntity
{
    public int Id { get; set; }

    public int SectionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public SectionEntity Section { get; set; } = null!;

    public ICollection<SubmissionEntity> Submissions { get; set; } = [];
}

public sealed class AttendanceRecordEntity
{
    public int Id { get; set; }

    public int ClassSessionId { get; set; }

    public int StudentId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime? CorrectedAt { get; set; }

    public ClassSessionEntity ClassSession { get; set; } = null!;

    public StudentEntity Student { get; set; } = null!;
}

public sealed class SubmissionEntity
{
    public int Id { get; set; }

    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public string Status { get; set; } = string.Empty;

    public AssignmentEntity Assignment { get; set; } = null!;

    public StudentEntity Student { get; set; } = null!;
}

public sealed class InterventionEntity
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public DateTime InterventionDate { get; set; }

    public string Channel { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string? Note { get; set; }

    public StudentEntity Student { get; set; } = null!;
}

public sealed class SettingEntity
{
    public string Key { get; set; } = string.Empty;

    public int Value { get; set; }
}
