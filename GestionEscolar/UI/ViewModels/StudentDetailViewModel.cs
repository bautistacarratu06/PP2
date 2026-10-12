using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UI.Models;

namespace UI.ViewModels
{
    // Recibe el alumno desde Seguimiento por parámetro de navegación ("Student").
    // TODO: Los historiales y la comisión están hardcodeados. A futuro vendrán de la base de datos
    // (attendance_record, submission, intervention).
    public partial class StudentDetailViewModel : ObservableObject, IQueryAttributable
    {
        [ObservableProperty]
        public partial string Name { get; set; } = "Gómez, Ana";

        [ObservableProperty]
        public partial string StudentNumber { get; set; } = "A1020";

        [ObservableProperty]
        public partial string Commission { get; set; } = "1°A";

        [ObservableProperty]
        public partial string Phone { get; set; } = "11 5555-1020";

        [ObservableProperty]
        public partial string Email { get; set; } = "estudiante20@ejemplo.edu.ar";

        [ObservableProperty]
        public partial string RiskLevel { get; set; } = "Alto";

        [ObservableProperty]
        public partial int RiskScore { get; set; } = 100;

        [ObservableProperty]
        public partial string Justification { get; set; } =
            "Asistió a 0 de 8 clases (0%) · Entregó 0 de 4 trabajos (0%) · Score 0 → Riesgo 100";

        [ObservableProperty]
        public partial Color BadgeBackground { get; set; } = Color.FromArgb("#FDECEC");

        [ObservableProperty]
        public partial Color BadgeForeground { get; set; } = Color.FromArgb("#E23D3D");

        public ObservableCollection<HistoryItem> Attendance { get; } =
        [
            new("Clase · 02/09/2026", "Ausente"),
            new("Clase · 03/09/2026", "Ausente"),
            new("Clase · 04/09/2026", "Ausente"),
            new("Clase · 05/09/2026", "Ausente"),
            new("Clase · 06/09/2026", "Ausente"),
            new("Clase · 07/09/2026", "Ausente"),
            new("Clase · 08/09/2026", "Ausente"),
            new("Clase · 09/09/2026", "Ausente")
        ];

        public ObservableCollection<HistoryItem> Submissions { get; } =
        [
            new("Trabajo práctico 1", "no entregó"),
            new("Trabajo práctico 2", "no entregó"),
            new("Trabajo práctico 3", "no entregó"),
            new("Trabajo práctico 4", "no entregó")
        ];

        public ObservableCollection<HistoryItem> Interventions { get; } = new();

        public bool HasNoInterventions => Interventions.Count == 0;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (!query.TryGetValue("Student", out var value) || value is not Student student)
                return;

            Name = student.Name;
            StudentNumber = student.StudentNumber;
            Phone = student.Phone;
            Email = student.Email;
            RiskLevel = student.RiskLevel;
            RiskScore = student.RiskScore;
            Justification = student.Justification;

            // TODO: mismos colores que StudentRow en TrackingViewModel; centralizarlos.
            (BadgeBackground, BadgeForeground) = student.RiskLevel switch
            {
                "Alto" => (Color.FromArgb("#FDECEC"), Color.FromArgb("#E23D3D")),
                "Medio" => (Color.FromArgb("#FFF3D6"), Color.FromArgb("#C9841A")),
                "Bajo" => (Color.FromArgb("#E5F6EC"), Color.FromArgb("#1E9D57")),
                _ => (Color.FromArgb("#F2F4F7"), Color.FromArgb("#5C6570"))
            };
        }
    }

    public sealed record HistoryItem(string Label, string Status);
}