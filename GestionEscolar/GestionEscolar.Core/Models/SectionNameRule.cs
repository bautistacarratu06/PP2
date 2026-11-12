namespace GestionEscolar.Core.Models;

// Valida el nombre de una comisión nueva contra el largo máximo y las ya existentes.
public static class SectionNameRule
{
    public const int MaxLength = StudentEntryRules.MaxSectionNameLength;

    public static string? Error(string? name, IEnumerable<Section> existing)
    {
        ArgumentNullException.ThrowIfNull(existing);
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            return "El nombre no puede estar vacío.";

        if (trimmed.Length > MaxLength)
            return $"El nombre no puede superar {MaxLength} caracteres.";

        if (existing.Any(section => section.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            return "Ya existe una comisión con ese nombre.";

        return null;
    }
}
