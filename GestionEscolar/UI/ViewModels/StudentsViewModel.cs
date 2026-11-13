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

    // Filas por página: la grilla no virtualiza, así que se agregan de a tandas al hacer scroll.
    public const int PageSize = 100;

    private readonly IStudentsQueryService _query;
    private readonly SectionSelection _selection;
    private readonly SectionChangeSubscription _subscription;
    private int? _boundSectionId;
    private int _loadedPage;
    private int _generation;

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
        Import.Imported += message => StatusMessage = message;
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

    // Total de filas que cumplen los filtros (el pie muestra "Cargados: n de total").
    [ObservableProperty]
    public partial int TotalMatching { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingMore { get; set; }

    [ObservableProperty]
    public partial string LoadMoreError { get; set; } = string.Empty;

    public int HighRiskCount => Summary.High;
    public int MediumRiskCount => Summary.Medium;
    public int LowRiskCount => Summary.Low;
    public int TotalCount => Summary.Total;
    public int FilteredCount => FilteredStudents.Count;
    public bool HasMore => FilteredStudents.Count < TotalMatching;
    public bool HasLoadMoreError => !string.IsNullOrEmpty(LoadMoreError);
    public string LoadedText => $"Cargados: {FilteredStudents.Count} de {TotalMatching}";

    public bool HasNoResults => !IsBusy && FilteredStudents.Count == 0 && string.IsNullOrEmpty(ErrorMessage);
    public bool ShowEmptyRoster => HasNoResults && !Filters.HasActiveFilters && string.IsNullOrWhiteSpace(SearchText);
    public bool ShowSearchEmpty => HasNoResults && (Filters.HasActiveFilters || !string.IsNullOrWhiteSpace(SearchText));
    public bool ShowTable => !ShowEmptyRoster && string.IsNullOrEmpty(ErrorMessage);

    public void Subscribe() => _subscription.Subscribe();

    public void Unsubscribe() => _subscription.Unsubscribe();

    partial void OnSearchTextChanged(string value) => _ = ReloadQuietAsync();

    partial void OnTotalMatchingChanged(int value) => NotifyListState();

    partial void OnLoadMoreErrorChanged(string value) => OnPropertyChanged(nameof(HasLoadMoreError));

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
        _generation++;
        LoadMoreError = string.Empty;
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

        var result = await _query.QueryAsync(section.Id, Filters.Build(SearchText, 1, PageSize));
        if (HasPendingWork)
            return;

        Summary = result.ActiveRiskSummary;
        _loadedPage = 1;
        Selection.ReplaceRows(result.Students.Select(student => new StudentRow(student, 0)));
        TotalMatching = result.TotalCount;
        NotifyListState();
    }

    // Trae la página siguiente y la agrega al final. Si mientras tanto hubo una recarga
    // (cambio de filtro, búsqueda o comisión), el resultado se descarta.
    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy || IsLoadingMore || !HasMore || _selection.Current is not { } section)
            return;

        var generation = _generation;
        IsLoadingMore = true;
        LoadMoreError = string.Empty;
        try
        {
            var filter = Filters.Build(SearchText, _loadedPage + 1, PageSize);
            var result = await _query.QueryAsync(section.Id, filter);
            if (generation != _generation || IsBusy)
                return;

            _loadedPage++;
            Selection.AppendRows(result.Students.Select(student => new StudentRow(student, 0)));
            TotalMatching = result.TotalCount;
            NotifyListState();
        }
        catch (Exception ex)
        {
            LoadMoreError = "No se pudieron cargar más estudiantes.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsLoadingMore = false;
        }
    }

    private void ClearRows()
    {
        Selection.ClearRows();
        _loadedPage = 0;
        TotalMatching = 0;
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
        OnPropertyChanged(nameof(HasMore));
        OnPropertyChanged(nameof(LoadedText));
        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(ShowEmptyRoster));
        OnPropertyChanged(nameof(ShowSearchEmpty));
        OnPropertyChanged(nameof(ShowTable));
    }
}
