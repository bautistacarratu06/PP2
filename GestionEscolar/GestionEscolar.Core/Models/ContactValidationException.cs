namespace GestionEscolar.Core.Models;

public sealed class ContactValidationException(string message) : Exception(message);
