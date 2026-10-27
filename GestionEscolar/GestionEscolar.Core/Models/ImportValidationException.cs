namespace GestionEscolar.Core.Models;

public sealed class ImportValidationException(string message) : Exception(message);
