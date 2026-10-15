using GestionEscolar.Core.Enums;

namespace UI.ViewModels
{
    // Colores del badge de riesgo, centralizados para Seguimiento y Detalle.
    // TODO: pasarlos a recursos XAML (Colors.xaml) cuando se defina el diseño.
    public static class RiskBadge
    {
        public static (Color Background, Color Foreground) For(RiskLevel? level) => level switch
        {
            RiskLevel.Alto => (Color.FromArgb("#FDECEC"), Color.FromArgb("#E23D3D")),
            RiskLevel.Medio => (Color.FromArgb("#FFF3D6"), Color.FromArgb("#C9841A")),
            RiskLevel.Bajo => (Color.FromArgb("#E5F6EC"), Color.FromArgb("#1E9D57")),
            _ => (Color.FromArgb("#F2F4F7"), Color.FromArgb("#5C6570"))
        };
    }
}
