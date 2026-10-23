namespace GestionEscolar.Core.Models;

// Clase de una comisión (dbo.class_session). La fecha la identifica: única por comisión.
public sealed record ClassSession(int Id, int SectionId, DateTime Date);
