using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

// Pantalla Estudiantes: carga la nómina con el riesgo y aloja los filtros, la selección,
// el alta y las acciones masivas, cada uno en su propio ViewModel hijo.
public partial class StudentsViewModel : BusyStateViewModel
{
    private const string LoadFailedMessage = "No se pudieron cargar los estudiantes.";

    private readonly IStudentsQueryService _query;
    private readonly SectionSelection _selection;
    private readonly SectionChangeSubscription _subscription;
    private int? _boundSectionId;

    public StudentsViewModel(
        IStudentsQueryService query,
        IStudentService students,
        IAttendanceService attendance,
        ISubmissionService submissions,
        SectionSelection selection,
        CommissionImportViewModel import,
        IClock clock)
    {
        _query = query;
        _selection = selection;
        Import = import;
        Selection = new StudentsSelectionViewModel(FilteredStudents);
        Filters = new StudentsFiltersViewModel();
        AddStudent = new AddStudentViewModel(students, selection, clock, ReloadAsync);
        Actions = new StudentsActionsViewModel(attendance, submissions, students, selection, Selection, clock, OnActionAppliedAsync);
        Filters.Changed += (_, _) => OnFiltersChanged();
        _subscription = new SectionChangeSubscription(selection, () => _ = LoadCommand.ExecuteAsync(null));
    }

    public CommissionImportViewModel Import { get; }

    public StudentsFiltersViewModel Filters { get; }

    public StudentsSelectionViewModel Selection { get; }

    public AddStudentViewModel AddStudent { get; }

    public StudentsActionsViewModel Actions { get; }

    public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HighRiskCount), nameof(MediumRiskCount), nameof(LowRiskCount), nameof(TotalCount))]
    public partial RiskSummary Summary { get; set; } = RiskSummary.Empty;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    public int HighRiskCount => Summary.High;
    public int MediumRiskCount => Summary.Medium;
    public int LowRiskCount => Summary.Low;
    public int TotalCount => Summary.Total;
    public int FilteredCount => FilteredStudents.Count;

    public bool HasNoResults => !IsBusy && FilteredStudents.Count == 0 && string.IsNullOrEmpty(ErrorMessage);
    public bool ShowEmptyRoster => HasNoResults && !Filters.HasActiveFilters && string.IsNullOrWhiteSpace(SearchText);
    public bool ShowSearchEmpty => HasNoResults && (Filters.HasActiveFilters || !string.IsNullOrWhiteSpace(SearchText));
    public bool ShowTable => !ShowEmptyRoster && string.IsNullOrEmpty(ErrorMessage);

    public void Subscribe() => _subscription.Subscribe();

    public void Unsubscribe() => _subscription.Unsubscribe();

    partial void OnSearchTextChanged(string value) => _ = ReloadQuietAsync();

    protected override void OnBusyChanged() => NotifyListState();

    protected override void OnWorkFailed() => ClearRows();

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(ErrorMessage))
            NotifyListState();
    }

    [RelayCommand]
    private Task LoadAsync() => RunLoopAsync(LoadRowsAsync, LoadFailedMessage);

    [RelayCommand]
    private Task OpenAsync(StudentRow row) =>
        Shell.Current.GoToAsync(nameof(Views.StudentDetailPage), new Dictionary<string, object>
        {
            [StudentDetailViewModel.StudentQueryKey] = row,
            [StudentDetailViewModel.SectionQueryKey] = _selection.Current?.Name ?? string.Empty,
            [StudentDetailViewModel.SectionIdQueryKey] = _selection.Current?.Id ?? 0
        });

    private async Task LoadRowsAsync()
    {
        await _selection.EnsureLoadedAsync();
        var section = _selection.Current;
        if (section is null)
        {
            Selection.Reset();
            _boundSectionId = null;
            ClearRows();
            return;
        }

        if (_boundSectionId != section.Id)
        {
            Selection.Reset();
            _boundSectionId = section.Id;
        }

        var result = await _query.QueryAsync(section.Id, Filters.Build(SearchText));
        if (HasPendingWork)
            return;

        Summary = result.ActiveRiskSummary;
        Selection.ReplaceRows(result.Students.Select(student => new StudentRow(student, 0)));
        NotifyListState();
    }

    private void ClearRows()
    {
        Selection.ClearRows();
        Summary = RiskSummary.Empty;
        NotifyListState();
    }

    private Task ReloadAsync() => LoadCommand.ExecuteAsync(null);

    private async Task OnActionAppliedAsync()
    {
        StatusMessage = "Acción aplicada correctamente.";
        await ReloadAsync();
    }

    private void OnFiltersChanged()
    {
        NotifyListState();
        _ = ReloadQuietAsync();
    }

    private async Task ReloadQuietAsync()
    {
        if (_selection.Current is null)
            return;
        await LoadCommand.ExecuteAsync(null);
    }

    private void NotifyListState()
    {
        OnPropertyChanged(nameof(FilteredCount));
        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(ShowEmptyRoster));
        OnPropertyChanged(nameof(ShowSearchEmpty));
        OnPropertyChanged(nameof(ShowTable));
    }
}
