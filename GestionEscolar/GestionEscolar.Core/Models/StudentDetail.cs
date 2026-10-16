namespace GestionEscolar.Core.Models;

// Ficha completa del alumno para la vista de detalle.
public class StudentDetail
{
    public int StudentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public IReadOnlyList<Submission> Submissions { get; set; } = [];
    public IReadOnlyList<Intervention> Interventions { get; set; } = [];
    public IReadOnlyList<AttendanceRecord> AttendanceRecords { get; set; } = [];

    public string FullName => $"{Name} {LastName}".Trim();
}
