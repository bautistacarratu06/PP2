namespace GestionEscolar.Core.Models;

// Contacto registrado con el alumno (dbo.intervention).
public class Intervention
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public DateTime Date { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public string? Note { get; set; }

    public string Description =>
        string.IsNullOrWhiteSpace(Note) ? Outcome : $"{Outcome} ({Note})";
}
