using GestionEscolar.Core.Enums;
using UI.Formatting;

namespace UI.ViewModels
{
    // Colores del badge de riesgo, tomados de RiskPalette para Seguimiento y Detalle.
    public static class RiskBadge
    {
        public static (Color Background, Color Foreground) For(RiskLevel? level)
        {
            var (ink, soft) = RiskPalette.For(level?.ToString());
            return (Color.FromArgb(soft), Color.FromArgb(ink));
        }
    }
}
