using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels
{
    public partial class TrackingViewModel : ObservableObject
    {
        private readonly TrackingService _trackingService;
        private readonly SectionSelection _selection;
        private bool _reloadPending;

        public TrackingViewModel(TrackingService trackingService, SectionSelection selection)
        {
            _trackingService = trackingService;
            _selection = selection;
            _selection.Changed += (_, _) => MainThread.BeginInvokeOnMainThread(() => _ = LoadCommand.ExecuteAsync(null));
        }

        public ObservableCollection<StudentRiskResult> Students { get; } = new();

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HighRiskCount), nameof(MediumRiskCount), nameof(LowRiskCount), nameof(TotalCount))]
        public partial RiskSummary Summary { get; set; } = RiskSummary.Empty;

        public int HighRiskCount => Summary.High;

        public int MediumRiskCount => Summary.Medium;

        public int LowRiskCount => Summary.Low;

        public int TotalCount => Summary.Total;

        public int FilteredCount => FilteredStudents.Count;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SelectedRisk { get; set; } = "Todos";

        public bool HasNoResults => !IsBusy && FilteredStudents.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

        public bool HighActive => SelectedRisk == "Alto";

        public bool MediumActive => SelectedRisk == "Medio";

        public bool LowActive => SelectedRisk == "Bajo";

        partial void OnSearchTextChanged(string value) => ApplyFilter();

        partial void OnSelectedRiskChanged(string value)
        {
            OnPropertyChanged(nameof(HighActive));
            OnPropertyChanged(nameof(MediumActive));
            OnPropertyChanged(nameof(LowActive));
            ApplyFilter();
        }

        [RelayCommand]
        private async Task LoadAsync()
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
                    ErrorMessage = string.Empty;
                    try
                    {
                        await _selection.EnsureLoadedAsync();
                        var section = _selection.Current;
                        if (section is null)
                        {
                            Students.Clear();
                            Summary = RiskSummary.Empty;
                            break;
                        }

                        var tracking = await _trackingService.GetSectionRiskAsync(section.Id);
                        if (_reloadPending)
                            continue;

                        Summary = tracking.Summary;
                        Students.Clear();
                        foreach (var result in tracking.Students.OrderByDescending(student => student.RiskScore))
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
                OnPropertyChanged(nameof(HasNoResults));
            }
        }

        [RelayCommand]
        private void ToggleRisk(string level) =>
            SelectedRisk = SelectedRisk == level ? "Todos" : level;

        [RelayCommand]
        private Task OpenAsync(StudentRow row) =>
            Shell.Current.GoToAsync(nameof(Views.StudentDetailPage), new Dictionary<string, object>
            {
                [StudentDetailViewModel.StudentQueryKey] = row,
                [StudentDetailViewModel.SectionQueryKey] = _selection.Current?.Name ?? string.Empty,
                [StudentDetailViewModel.SectionIdQueryKey] = _selection.Current?.Id ?? 0
            });

        private void ApplyFilter()
        {
            var query = SearchText.Trim();
            IEnumerable<StudentRiskResult> rows = Students;
            if (SelectedRisk is "Alto" or "Medio" or "Bajo" && Enum.TryParse<RiskLevel>(SelectedRisk, out var level))
                rows = rows.Where(student => student.RiskLevel == level);

            if (query.Length > 0)
            {
                rows = rows.Where(student =>
                    student.Data.Student.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                    || student.Data.Student.StudentNumber.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            FilteredStudents.Clear();
            foreach (var student in rows)
                FilteredStudents.Add(new StudentRow(student, 0));

            OnPropertyChanged(nameof(FilteredCount));
            OnPropertyChanged(nameof(HasNoResults));
        }
    }
}
