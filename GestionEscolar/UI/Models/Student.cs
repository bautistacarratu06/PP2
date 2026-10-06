using System.Globalization;

namespace UI.Models
{
    // TODO: Esto no es un estudiante, es un resumen de un estudiante. Debería llamarse StudentSummary o algo así.
    // Es fabricación pura para la UI, no debería estar en el dominio. Debería ser un DTO que se construye a partir de un Student y sus relaciones.
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public int RiskScore { get; set; }
        public int AttendedClasses { get; set; }
        public int TotalClasses { get; set; }
        public int SubmittedAssignments { get; set; }
        public int TotalAssignments { get; set; }
        public DateTime? LastContact { get; set; }

        // TODO: Implemntar algoritmo de riesgo de Bautista, que debería estar en otra clase.
        // Por ahora esto queda como placeholder para que la UI pueda mostrar algo. 
        public int AttendancePercent =>
            TotalClasses == 0
                ? 0
                : (int)Math.Round(AttendedClasses * 100.0 / TotalClasses, MidpointRounding.AwayFromZero);

        // TODO: Implemntar algoritmo de riesgo de Bautista, que debería estar en otra clase.
        // Por ahora esto queda como placeholder para que la UI pueda mostrar algo. 
        public int DeliveryPercent =>
            TotalAssignments == 0
                ? 0
                : (int)Math.Round(SubmittedAssignments * 100.0 / TotalAssignments, MidpointRounding.AwayFromZero);

        // TODO: Implemntar algoritmo de riesgo de Bautista, que debería estar en otra clase.
        // Por ahora esto queda como placeholder para que la UI pueda mostrar algo. 
        public int Score => Math.Clamp(100 - RiskScore, 0, 100);

        public string AttendanceText => $"{AttendancePercent}%";

        public string DeliveryText => $"{DeliveryPercent}%";

        public string LegajoLabel => $"Legajo {StudentNumber}";

        public string ContactLabel =>
            LastContact is null
                ? "Sin contactar"
                : LastContact.Value.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture);

        public string Justification =>
            $"Asistió a {AttendedClasses} de {TotalClasses} clases ({AttendancePercent}%) · Entregó {SubmittedAssignments} de {TotalAssignments} trabajos ({DeliveryPercent}%) · Score {Score} → Riesgo {RiskScore}";
    }
}
