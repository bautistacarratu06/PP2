using System.Globalization;

namespace UI.Formatting;

public static class ContactWhen
{
    public static string Relative(DateTime? date)
    {
        if (date is null)
            return "Sin contactar";

        var days = (DateTime.Today - date.Value.Date).Days;
        if (days <= 0)
            return "Hoy";
        if (days == 1)
            return "Ayer";
        if (days < 7)
            return $"Hace {days} días";
        if (days < 14)
            return "Hace 1 sem.";
        if (days < 30)
        {
            var weeks = days / 7;
            return weeks <= 1 ? "Hace 1 sem." : $"Hace {weeks} sem.";
        }

        return date.Value.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);
    }
}
