namespace GestionEscolar.Core.Models;

// Umbrales de riesgo en memoria. RiskSettingsService los carga y los actualiza
// desde dbo.setting. 50 y 25 coinciden con el valor inicial de la tabla.
public class RiskConfig
{
    // Riesgo igual o mayor a este valor = Alto
    public int HighThreshold { get; set; } = 50;

    // Riesgo igual o mayor a este valor (y menor al alto) = Medio
    public int MediumThreshold { get; set; } = 25;
}
