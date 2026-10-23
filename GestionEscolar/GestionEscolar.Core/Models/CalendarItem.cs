using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Ítem de la lista combinada del calendario. Name es null para las clases (no tienen nombre).
public sealed record CalendarItem(int Id, CalendarItemKind Kind, string? Name, DateTime Date);
