namespace UI.Formatting;

// Colores por nivel de riesgo (tinta y fondo suave), compartidos por badge, pill y tarjeta KPI.
public static class RiskPalette
{
    public static (string Ink, string Soft) For(string? level) => level switch
    {
        "Alto" => ("#F87171", "#1FF87171"),
        "Medio" => ("#FBBF24", "#1FFBBF24"),
        "Bajo" => ("#34D399", "#1F34D399"),
        _ => ("#8E8E93", "#1B1B1B")
    };
}
