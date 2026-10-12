using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UI.Models;
using UI.Services;

namespace UI.ViewModels
{
    public partial class TrackingViewModel : ObservableObject
    {
        
        // // TODO: Los totales están fijos. A futuro vendrán de la base de datos.
        // private readonly ScoringRiskServices _scoringRiskServices = new()
        // {
        //     TotalClasses = 8,
        //     TotalAssignments = 4
        // };

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

        // TODO: la comisión está fija en 1 hasta que haya un selector de comisión en la UI.
        private const int SectionId = 1;

        // Carga las filas desde la base (reemplaza al seeder del CSV).
        // Si la base no responde dejamos la grilla vacía: una excepción en el constructor
        // de la página deja el Shell en blanco.
        private void GetMockedStudentsRowRenameThisMethodUponDbImplementation()
        {
            List<Student> students;
            try
            {
                students = GetTrakingRowsBySection.GetRows(SectionId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los alumnos: {ex.Message}");
                return;
            }

            foreach (var student in students)
                Students.Add(student);
        }
    }
    public sealed class StudentRow
    {
        public StudentRow(Student student, int rowNumber)
        {
            Student = student;
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

        // Alumno original, se pasa a la vista de detalle al tocar la fila.
        public Student Student { get; }
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
