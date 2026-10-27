using System.Globalization;

namespace UI.Formatting;

// El nombre llega como "Nombre Apellido" (nómina y seguimiento) o ya como "Apellido, Nombre"
// (asistencia y entregas). La lista muestra siempre apellido primero.
public static class PersonName
{
    public static string AsSurnameFirst(string? fullName)
    {
        var name = (fullName ?? string.Empty).Trim();
        if (name.Length == 0 || name.Contains(','))
            return name;

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return name;

        var surname = parts[^1];
        var given = string.Join(' ', parts[..^1]);
        return $"{surname}, {given}";
    }

    public static string AsGivenFirst(string? fullName)
    {
        var name = (fullName ?? string.Empty).Trim();
        if (!name.Contains(','))
            return name;

        var parts = name.Split(',', 2, StringSplitOptions.TrimEntries);
        return parts.Length < 2 || parts[1].Length == 0 ? parts[0] : $"{parts[1]} {parts[0]}";
    }

    public static string Initials(string? surnameFirst)
    {
        var name = (surnameFirst ?? string.Empty).Trim();
        if (name.Length == 0)
            return string.Empty;

        if (name.Contains(','))
        {
            var parts = name.Split(',', 2, StringSplitOptions.TrimEntries);
            var given = parts.Length > 1 ? parts[1] : string.Empty;
            return Letter(given) + Letter(parts[0]);
        }

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length == 1 ? Letter(words[0]) : Letter(words[0]) + Letter(words[^1]);
    }

    private static string Letter(string word) =>
        word.Length == 0 ? string.Empty : char.ToUpper(word[0], CultureInfo.InvariantCulture).ToString();
}
