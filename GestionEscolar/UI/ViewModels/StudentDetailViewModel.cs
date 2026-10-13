using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionEscolar.Core.Enums;

namespace UI.ViewModels
{
    // Recibe la fila desde Seguimiento por parámetro de navegación ("Student", un StudentRow).
    // TODO: Los historiales y la comisión están hardcodeados. A futuro vendrán de la base de datos
    // (attendance_record, submission, intervention).
    public partial class StudentDetailViewModel : ObservableObject, IQueryAttributable
    {
        public const string StudentQueryKey = "Student";

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
        public partial string RiskLevel { get; set; } = nameof(GestionEscolar.Core.Enums.RiskLevel.Alto);

        [ObservableProperty]
        public partial int RiskScore { get; set; } = 100;

        [ObservableProperty]
        public partial string Justification { get; set; } =
            "Asistió a 0 de 8 clases (0%) · Entregó 0 de 4 trabajos (0%) · Score 0 → Riesgo 100";

        [ObservableProperty]
        public partial Color BadgeBackground { get; set; } = RiskBadge.For(GestionEscolar.Core.Enums.RiskLevel.Alto).Background;

        [ObservableProperty]
        public partial Color BadgeForeground { get; set; } = RiskBadge.For(GestionEscolar.Core.Enums.RiskLevel.Alto).Foreground;

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
            new("Trabajo práctico 1", SubmissionStatus.NoEntrego.ToDbValue()),
            new("Trabajo práctico 2", SubmissionStatus.NoEntrego.ToDbValue()),
            new("Trabajo práctico 3", SubmissionStatus.NoEntrego.ToDbValue()),
            new("Trabajo práctico 4", SubmissionStatus.NoEntrego.ToDbValue())
        ];

        public ObservableCollection<HistoryItem> Interventions { get; } = new();

        public bool HasNoInterventions => Interventions.Count == 0;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (!query.TryGetValue(StudentQueryKey, out var value) || value is not StudentRow row)
                return;

            Name = row.Name;
            StudentNumber = row.StudentNumber;
            Phone = row.Phone;
            Email = row.Email;
            RiskLevel = row.RiskLevel;
            RiskScore = row.RiskScore;
            Justification = row.Justification;
            BadgeBackground = row.BadgeBackground;
            BadgeForeground = row.BadgeForeground;
        }
    }

    public sealed record HistoryItem(string Label, string Status);
}
