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
        private readonly SectionService _sectionService;

        public TrackingViewModel(TrackingService trackingService, SectionService sectionService)
        {
            _trackingService = trackingService;
            _sectionService = sectionService;
            SelectedRisk = AllFilter;
        }

        // Comisiones para el selector del encabezado.
        public ObservableCollection<Section> Sections { get; } = new();

        [ObservableProperty]
        public partial Section? SelectedSection { get; set; }

        private bool _suppressSectionReload;
        private bool _reloadPending;

        public ObservableCollection<StudentRiskResult> Students { get; } = new();

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        public IReadOnlyList<string> RiskFilterOptions { get; } = RiskFilters.Keys.ToList();

        // Tarjetas Alto / Medio / Bajo: salen del mismo resultado que la grilla.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HighRiskCount), nameof(MediumRiskCount), nameof(LowRiskCount))]
        public partial RiskSummary Summary { get; set; } = RiskSummary.Empty;

        public int HighRiskCount => Summary.High;

        public int MediumRiskCount => Summary.Medium;

        public int LowRiskCount => Summary.Low;

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

        // Se dispara desde TrackingPage.OnAppearing: carga las comisiones (la primera vez) y los datos
        // de la comisión seleccionada. Si la base no responde, la grilla queda vacía y se loguea.
        [RelayCommand]
        private async Task LoadAsync()
        {
            if (Sections.Count == 0)
            {
                try
                {
                    var sections = await _sectionService.GetAllAsync();
                    foreach (var section in sections)
                        Sections.Add(section);

                    // Asignación inicial sin disparar OnSelectedSectionChanged (evita doble carga).
                    _suppressSectionReload = true;
                    SelectedSection = SectionService.PickDefault(sections);
                    _suppressSectionReload = false;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "No se pudieron cargar las comisiones.";
                    System.Diagnostics.Debug.WriteLine($"No se pudieron cargar las comisiones: {ex}");
                    return;
                }
            }

            await LoadSectionDataAsync();
        }

        partial void OnSelectedSectionChanged(Section? value)
        {
            if (_suppressSectionReload || value is null)
                return;

            _ = LoadSectionDataAsync();
        }

        // Carga grilla + resumen de la comisión seleccionada. Si se cambia de comisión mientras
        // hay una carga en curso, se marca como pendiente y se recarga al terminar (sin cargas paralelas).
        private async Task LoadSectionDataAsync()
        {
            if (IsBusy)
            {
                _reloadPending = true;
                return;
            }

            IsBusy = true;
            try
            {
                do
                {
                    _reloadPending = false;
                    var section = SelectedSection;
                    if (section is null)
                        break;

                    ErrorMessage = string.Empty;
                    try
                    {
                        var tracking = await _trackingService.GetSectionRiskAsync(section.Id);

                        // Si mientras tanto cambió la comisión, descartamos este resultado.
                        if (_reloadPending)
                            continue;

                        Summary = tracking.Summary;

                        Students.Clear();
                        foreach (var result in tracking.Students)
                            Students.Add(result);
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = "No se pudieron cargar los alumnos.";
                        System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los alumnos: {ex}");
                        Summary = RiskSummary.Empty;
                        Students.Clear();
                    }

                    ApplyFilter();
                }
                while (_reloadPending);
            }
            finally
            {
                IsBusy = false;
            }
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
