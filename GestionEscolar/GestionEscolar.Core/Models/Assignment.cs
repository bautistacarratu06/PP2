namespace GestionEscolar.Core.Models;

// Trabajo de una comisión (dbo.assignment).
public sealed record Assignment(int Id, int SectionId, string Name, DateTime DueDate);
