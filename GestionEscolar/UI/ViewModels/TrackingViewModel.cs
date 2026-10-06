using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UI.Models;
using UI.Seeding;

namespace UI.ViewModels
{
    public partial class TrackingViewModel : ObservableObject
    {
        // TODO: Sacar esto una vez que tengamos la base de datos, y que el ID sea generado por la base de datos.
        private int _nextId = 1;

        public TrackingViewModel()
        {
            GetMockedStudentsRowRenameThisMethodUponDbImplementation();
            SelectedRisk = "Todos";
        }

        public ObservableCollection<Student> Students { get; } = new();

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        public IReadOnlyList<string> RiskFilterOptions { get; } =
        [
            // TODO: Esta lista de estados se repite, quizás debería estar en un archivo de configuración, o en un enum, o en algún lugar centralizado.
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
        // Fase 1: importar el CSV de la carga manual
        // Fase 2: realizar query a la base de datos
        // La fase 1 sirve de todos modos, ya que populamos masivamente la base de datos a partir de un CSV.
        // No desperdiciamos trabajo.
        private void GetMockedStudentsRowRenameThisMethodUponDbImplementation()
        {
            foreach (var student in StudentSeeder.GetMockedStudents())
            {
                // TODO: El ID está hardcodeado, debería ser generado por la base de datos. Esto es solo para poder mostrar la UI.
                student.Id = _nextId++;
                Students.Add(student);
            }
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
                // TODO: Estos colores deberían estar en un archivo de configuración, no hardcodeados. 
                // Debería ser parte del diseño de la UI, no de la lógica de negocio.
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
