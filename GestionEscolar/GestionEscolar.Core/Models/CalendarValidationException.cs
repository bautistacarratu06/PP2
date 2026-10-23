namespace GestionEscolar.Core.Models;

// Error de validación al agregar clases o trabajos (mensaje listo para mostrar al usuario).
public sealed class CalendarValidationException(string message) : Exception(message);
