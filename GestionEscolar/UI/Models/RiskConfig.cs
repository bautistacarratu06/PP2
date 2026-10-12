namespace UI.Models;

public class RiskConfig
{
    // Riesgo igual o mayor a este valor = Alto
    public int HighThreshold { get; set; } = 50;

    // Riesgo igual o mayor a este valor (y menor al alto) = Medio
    public int MediumThreshold { get; set; } = 25;
}