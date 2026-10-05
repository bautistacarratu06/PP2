using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UI.Models;

namespace UI.ViewModels
{
    public partial class TrackingViewModel : ObservableObject
    {
        private int _nextId = 1;

        public TrackingViewModel()
        {
            SeedStudents();
            SelectedRisk = "Todos";
        }

        public ObservableCollection<Student> Students { get; } = new();

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        public IReadOnlyList<string> RiskFilterOptions { get; } =
        [
            "Todos",
            "Riesgo Alto",
            "Riesgo Medio",
            "Riesgo Bajo"
        ];

        public int HighRiskCount => Students.Count(student => student.RiskLevel == "Alto");

        public int MediumRiskCount => Students.Count(student => student.RiskLevel == "Medio");

        public int LowRiskCount => Students.Count(student => student.RiskLevel == "Bajo");

        public int FilteredCount => FilteredStudents.Count;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredCount))]
        public partial bool OnlyWithoutContact { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FilteredCount))]
        public partial string SelectedRisk { get; set; }

        [RelayCommand]
        private void ToggleWithoutContact()
        {
            OnlyWithoutContact = !OnlyWithoutContact;
        }

        partial void OnOnlyWithoutContactChanged(bool value) => ApplyFilter();

        partial void OnSelectedRiskChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<Student> query = Students;

            if (OnlyWithoutContact)
                query = query.Where(student => student.LastContact is null);

            query = SelectedRisk switch
            {
                "Riesgo Alto" => query.Where(student => student.RiskLevel == "Alto"),
                "Riesgo Medio" => query.Where(student => student.RiskLevel == "Medio"),
                "Riesgo Bajo" => query.Where(student => student.RiskLevel == "Bajo"),
                _ => query
            };

            FilteredStudents.Clear();

            var rowNumber = 1;
            foreach (var student in query)
                FilteredStudents.Add(new StudentRow(student, rowNumber++));

            OnPropertyChanged(nameof(FilteredCount));
        }

        // TODO: Vera va a implementar otra fuente basada en .csv, por ahora se hace un seed de datos para poder mostrar la vista.
        private void SeedStudents()
        {
            Add("Gómez, Ana", "A1020", "Alto", 100, 0, 0);
            Add("Acosta, Elena", "A1004", "Alto", 60, 0, 4);
            Add("Acosta, Elena", "A1024", "Alto", 60, 0, 4);
            Add("Pérez, Bruno", "A1016", "Alto", 60, 0, 4);
            Add("Rossi, Diego", "A1008", "Alto", 60, 0, 4);
            Add("Rossi, Diego", "A1028", "Alto", 60, 0, 4);
            Add("Sosa, Camila", "A1012", "Alto", 60, 0, 4);
            Add("Gómez, Ana", "A1005", "Medio", 40, 8, 0);
            Add("Gómez, Ana", "A1010", "Medio", 40, 8, 0);
            Add("Torres, Valentina", "A1033", "Medio", 40, 8, 0);
            Add("Ruiz, Joaquín", "A1035", "Medio", 40, 8, 0, new DateTime(2026, 9, 15));
            Add("Molina, Sofía", "A1037", "Medio", 40, 8, 0);

            Add("Navarro, Lucía", "A1041", "Bajo", 10, 8, 4, new DateTime(2026, 9, 18));
            Add("Castro, Mateo", "A1043", "Bajo", 12, 8, 3);
            Add("Romero, Juana", "A1045", "Bajo", 15, 7, 4, new DateTime(2026, 9, 22));
            Add("Vargas, Nicolás", "A1047", "Bajo", 10, 8, 4);
            Add("Herrera, Camila", "A1049", "Bajo", 18, 8, 3, new DateTime(2026, 9, 5));
            Add("Medina, Thiago", "A1051", "Bajo", 14, 7, 4);
            Add("Ortiz, Valentina", "A1053", "Bajo", 12, 8, 4, new DateTime(2026, 8, 28));
            Add("Silva, Benjamín", "A1055", "Bajo", 16, 8, 3);
            Add("Ibáñez, Martina", "A1057", "Bajo", 20, 7, 4, new DateTime(2026, 9, 15));
            Add("Cabrera, Santiago", "A1059", "Bajo", 10, 8, 4);
            Add("Rojas, Emilia", "A1061", "Bajo", 15, 8, 3);
            Add("Paredes, Facundo", "A1063", "Bajo", 18, 7, 4, new DateTime(2026, 9, 1));
            Add("Aguirre, Renata", "A1065", "Bajo", 11, 8, 4);
            Add("Figueroa, Lautaro", "A1067", "Bajo", 14, 8, 3, new DateTime(2026, 9, 20));
            Add("Benítez, Catalina", "A1069", "Bajo", 17, 7, 4);
            Add("Morales, Ignacio", "A1071", "Bajo", 10, 8, 4);
            Add("Delgado, Paula", "A1073", "Bajo", 19, 8, 3, new DateTime(2026, 8, 30));
            Add("Vega, Tomás", "A1075", "Bajo", 13, 7, 4);
        }

        private void Add(
            string name,
            string studentNumber,
            string riskLevel,
            int riskScore,
            int attended,
            int submitted,
            DateTime? lastContact = null)
        {
            Students.Add(new Student
            {
                Id = _nextId++,
                Name = name,
                StudentNumber = studentNumber,
                RiskLevel = riskLevel,
                RiskScore = riskScore,
                AttendedClasses = attended,
                TotalClasses = 8,
                SubmittedAssignments = submitted,
                TotalAssignments = 4,
                LastContact = lastContact
            });
        }
    }

    public sealed class StudentRow
    {
        public StudentRow(Student student, int rowNumber)
        {
            RowNumber = rowNumber;
            Name = student.Name;
            LegajoLabel = student.LegajoLabel;
            RiskLevel = student.RiskLevel;
            RiskScore = student.RiskScore;
            AttendanceText = student.AttendanceText;
            DeliveryText = student.DeliveryText;
            Justification = student.Justification;
            ContactLabel = student.ContactLabel;

            (BadgeBackground, BadgeForeground, BadgeGlyph) = student.RiskLevel switch
            {
                "Alto" => (Color.FromArgb("#FDECEC"), Color.FromArgb("#E23D3D"), "●"),
                "Medio" => (Color.FromArgb("#FFF3D6"), Color.FromArgb("#C9841A"), "▲"),
                "Bajo" => (Color.FromArgb("#E5F6EC"), Color.FromArgb("#1E9D57"), "●"),
                _ => (Color.FromArgb("#F2F4F7"), Color.FromArgb("#5C6570"), "●")
            };
        }

        public int RowNumber { get; }
        public string Name { get; }
        public string LegajoLabel { get; }
        public string RiskLevel { get; }
        public int RiskScore { get; }
        public string AttendanceText { get; }
        public string DeliveryText { get; }
        public string Justification { get; }
        public string ContactLabel { get; }
        public Color BadgeBackground { get; }
        public Color BadgeForeground { get; }
        public string BadgeGlyph { get; }
    }
}
