using System.Globalization;

namespace UI.Models
{
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

        public int AttendancePercent =>
            TotalClasses == 0
                ? 0
                : (int)Math.Round(AttendedClasses * 100.0 / TotalClasses, MidpointRounding.AwayFromZero);

        public int DeliveryPercent =>
            TotalAssignments == 0
                ? 0
                : (int)Math.Round(SubmittedAssignments * 100.0 / TotalAssignments, MidpointRounding.AwayFromZero);

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
