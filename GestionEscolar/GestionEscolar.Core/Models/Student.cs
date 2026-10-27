namespace GestionEscolar.Core.Models;

// Datos del alumno tal como vienen de la base / CSV. Sin campos calculados ni formato.
public class Student
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrolledOn { get; set; }
}
