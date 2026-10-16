using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Un trabajo (assignment) de la comisión y la entrega del alumno.
public class Submission
{
    // id de submission; null si no hay registro para ese trabajo.
    public int? Id { get; set; }
    public int StudentId { get; set; }
    public int SectionId { get; set; }
    public int AssignmentId { get; set; }
    public string AssignmentName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }

    // null = el trabajo existe pero no hay registro de entrega para el alumno.
    public SubmissionStatus? Status { get; set; }

    public bool IsSubmitted => Status == SubmissionStatus.Entrego;
}
