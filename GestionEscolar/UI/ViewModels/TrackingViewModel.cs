using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.ViewModels
{
    public partial class TrackingViewModel : ObservableObject
    {
        // TODO: la comisión está fija en 1 hasta que haya un selector de comisión en la UI.
        private const int SectionId = 1;

        private const string AllFilter = "Todos";

        // Opción del filtro -> nivel de riesgo (null = todos)
        private static readonly IReadOnlyDictionary<string, RiskLevel?> RiskFilters = new Dictionary<string, RiskLevel?>
        {
            [AllFilter] = null,
            ["Riesgo Alto"] = RiskLevel.Alto,
            ["Riesgo Medio"] = RiskLevel.Medio,
            ["Riesgo Bajo"] = RiskLevel.Bajo
        };

        private readonly TrackingService _trackingService;

        public TrackingViewModel(TrackingService trackingService)
        {
            _trackingService = trackingService;
            SelectedRisk = AllFilter;
        }

        public ObservableCollection<StudentRiskResult> Students { get; } = new();

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        public IReadOnlyList<string> RiskFilterOptions { get; } = RiskFilters.Keys.ToList();

        public int HighRiskCount => Students.Count(s => s.RiskLevel == RiskLevel.Alto);

        public int MediumRiskCount => Students.Count(s => s.RiskLevel == RiskLevel.Medio);

        public int LowRiskCount => Students.Count(s => s.RiskLevel == RiskLevel.Bajo);

        public int FilteredCount => FilteredStudents.Count;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

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

        // Se dispara desde TrackingPage.OnAppearing. Si la base no responde, la grilla queda vacía
        // y el error se loguea (no se tira excepción: rompería el Shell).
        [RelayCommand]
        private async Task LoadAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;
            try
            {
                var results = await _trackingService.GetSectionRiskAsync(SectionId);

                Students.Clear();
                foreach (var result in results)
                    Students.Add(result);
            }
            catch (Exception ex)
            {
                ErrorMessage = "No se pudieron cargar los alumnos.";
                System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los alumnos: {ex}");
            }
            finally
            {
                IsBusy = false;
            }

            OnPropertyChanged(nameof(HighRiskCount));
            OnPropertyChanged(nameof(MediumRiskCount));
            OnPropertyChanged(nameof(LowRiskCount));
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<StudentRiskResult> query = Students;

            if (OnlyWithoutContact)
                query = query.Where(s => s.Data.LastContact is null);

            if (SelectedRisk is not null && RiskFilters.TryGetValue(SelectedRisk, out var level) && level is not null)
                query = query.Where(s => s.RiskLevel == level);

            FilteredStudents.Clear();

            var rowNumber = 1;
            foreach (var student in query)
                FilteredStudents.Add(new StudentRow(student, rowNumber++));

            OnPropertyChanged(nameof(FilteredCount));
        }
    }
}
