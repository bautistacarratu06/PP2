using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

public partial class StudentsViewModel : ObservableObject
{
    private readonly StudentsQueryService _query;
    private readonly StudentService _students;
    private readonly AttendanceService _attendance;
    private readonly SubmissionService _submissions;
    private readonly SectionSelection _selection;
    private readonly HashSet<int> _selectedIds = [];
    private bool _isUpdatingSelection;
    private bool _reloadPending;
    private int? _boundSectionId;

    public StudentsViewModel(
        StudentsQueryService query,
        StudentService students,
        AttendanceService attendance,
        SubmissionService submissions,
        SectionSelection selection,
        CommissionImportViewModel import)
    {
        _query = query;
        _students = students;
        _attendance = attendance;
        _submissions = submissions;
        _selection = selection;
        Import = import;
        _selection.Changed += (_, _) => MainThread.BeginInvokeOnMainThread(() => _ = LoadCommand.ExecuteAsync(null));
        RebuildActionCatalog();
    }

    public CommissionImportViewModel Import { get; }

    public bool CanPerformBulkActions => true;

    public DateTime Today => DateTime.Today;

    public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

    public ObservableCollection<ActionPaletteItem> FilteredActions { get; } = new();

    public ObservableCollection<PickerOption<ClassSession>> ClassSessions { get; } = new();

    public ObservableCollection<PickerOption<Assignment>> Assignments { get; } = new();

    private readonly List<ActionPaletteItem> _allActions = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HighRiskCount), nameof(MediumRiskCount), nameof(LowRiskCount), nameof(TotalCount))]
    public partial RiskSummary Summary { get; set; } = RiskSummary.Empty;

    public int HighRiskCount => Summary.High;
    public int MediumRiskCount => Summary.Medium;
    public int LowRiskCount => Summary.Low;
    public int TotalCount => Summary.Total;
    public int FilteredCount => FilteredStudents.Count;

    public int SelectedCount => _selectedIds.Count;
    public bool HasSelection => SelectedCount > 0;
    public string SelectedCountText =>
        $"{SelectedCount} {(SelectedCount == 1 ? "estudiante seleccionado" : "estudiantes seleccionados")}";

    public bool IsAllSelected
    {
        get => FilteredStudents.Count > 0 && FilteredStudents.All(row => row.IsSelected);
        set => SetAllSelected(value);
    }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string NameFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LegajoFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AttendanceMinText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AttendanceMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeliveryMinText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeliveryMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool FilterRiskAlto { get; set; }

    [ObservableProperty]
    public partial bool FilterRiskMedio { get; set; }

    [ObservableProperty]
    public partial bool FilterRiskBajo { get; set; }

    [ObservableProperty]
    public partial bool FilterContactSin { get; set; }

    [ObservableProperty]
    public partial bool FilterContactCon { get; set; }

    [ObservableProperty]
    public partial string ActiveFilterKey { get; set; } = "Activos";

    [ObservableProperty]
    public partial StudentsSortField SortBy { get; set; } = StudentsSortField.Name;

    [ObservableProperty]
    public partial bool SortDescending { get; set; }

    [ObservableProperty]
    public partial string OpenFilterColumn { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsAdding { get; set; }

    [ObservableProperty]
    public partial string LastName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FirstName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StudentNumber { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Dni { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime EnrolledOn { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddError { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsActionsOpen { get; set; }

    [ObservableProperty]
    public partial int ActionsStep { get; set; }

    [ObservableProperty]
    public partial string ActionSearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ActionPaletteItem? SelectedAction { get; set; }

    [ObservableProperty]
    public partial int HighlightedActionIndex { get; set; }

    [ObservableProperty]
    public partial PickerOption<ClassSession>? SelectedClassSession { get; set; }

    [ObservableProperty]
    public partial PickerOption<Assignment>? SelectedAssignment { get; set; }

    [ObservableProperty]
    public partial string ActionError { get; set; } = string.Empty;

    public bool HasNoResults => !IsBusy && FilteredStudents.Count == 0 && string.IsNullOrEmpty(ErrorMessage);
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);
    public bool HasAddError => !string.IsNullOrEmpty(AddError);
    public bool HasActionError => !string.IsNullOrEmpty(ActionError);
    public bool ShowEmptyRoster => HasNoResults && !HasActiveFilters && string.IsNullOrWhiteSpace(SearchText);
    public bool ShowSearchEmpty => HasNoResults && (HasActiveFilters || !string.IsNullOrWhiteSpace(SearchText));
    public bool ShowTable => !ShowEmptyRoster && string.IsNullOrEmpty(ErrorMessage);
    public bool HighActive => FilterRiskAlto && !FilterRiskMedio && !FilterRiskBajo;
    public bool MediumActive => FilterRiskMedio && !FilterRiskAlto && !FilterRiskBajo;
    public bool LowActive => FilterRiskBajo && !FilterRiskAlto && !FilterRiskMedio;
    public bool HasActiveFilters => BuildFilter(includeSearch: false) is { } f && HasColumnFilters(f);
    public bool IsActionsStep1 => ActionsStep == 0;
    public bool IsActionsStep2 => ActionsStep == 1;
    public bool IsAttendanceAction => SelectedAction?.Kind == StudentsActionKind.Attendance;
    public bool IsSubmissionsAction => SelectedAction?.Kind == StudentsActionKind.Submissions;
    public bool IsDeactivateAction => SelectedAction?.Kind == StudentsActionKind.Deactivate;
    public string DeactivateConfirmText =>
        $"Vas a desactivar {SelectedCount} {(SelectedCount == 1 ? "estudiante" : "estudiantes")}. " +
        "Quedarán ocultos por defecto y se podrán volver a ver con el filtro de Estado.";

    partial void OnSearchTextChanged(string value) => _ = ReloadQuietAsync();
    partial void OnNameFilterChanged(string value) => NotifyFiltersAndReload();
    partial void OnLegajoFilterChanged(string value) => NotifyFiltersAndReload();
    partial void OnEmailFilterChanged(string value) => NotifyFiltersAndReload();
    partial void OnAttendanceMinTextChanged(string value) => NotifyFiltersAndReload();
    partial void OnAttendanceMaxTextChanged(string value) => NotifyFiltersAndReload();
    partial void OnDeliveryMinTextChanged(string value) => NotifyFiltersAndReload();
    partial void OnDeliveryMaxTextChanged(string value) => NotifyFiltersAndReload();
    partial void OnFilterRiskAltoChanged(bool value) => NotifyRiskAndReload();
    partial void OnFilterRiskMedioChanged(bool value) => NotifyRiskAndReload();
    partial void OnFilterRiskBajoChanged(bool value) => NotifyRiskAndReload();
    partial void OnFilterContactSinChanged(bool value) => NotifyFiltersAndReload();
    partial void OnFilterContactConChanged(bool value) => NotifyFiltersAndReload();
    partial void OnActiveFilterKeyChanged(string value) => NotifyFiltersAndReload();
    partial void OnSortByChanged(StudentsSortField value) => _ = ReloadQuietAsync();
    partial void OnSortDescendingChanged(bool value) => _ = ReloadQuietAsync();
    partial void OnAddErrorChanged(string value) => OnPropertyChanged(nameof(HasAddError));
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
    partial void OnStatusMessageChanged(string value) => OnPropertyChanged(nameof(HasStatus));
    partial void OnActionErrorChanged(string value) => OnPropertyChanged(nameof(HasActionError));
    partial void OnActionsStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsActionsStep1));
        OnPropertyChanged(nameof(IsActionsStep2));
    }

    partial void OnSelectedActionChanged(ActionPaletteItem? value)
    {
        OnPropertyChanged(nameof(IsAttendanceAction));
        OnPropertyChanged(nameof(IsSubmissionsAction));
        OnPropertyChanged(nameof(IsDeactivateAction));
        OnPropertyChanged(nameof(DeactivateConfirmText));
    }

    partial void OnActionSearchTextChanged(string value) => ApplyActionFilter();

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
                        FilteredStudents.Clear();
                        Summary = RiskSummary.Empty;
                        _selectedIds.Clear();
                        _boundSectionId = null;
                        NotifySelectionChanged();
                        break;
                    }

                    if (_boundSectionId != section.Id)
                    {
                        _selectedIds.Clear();
                        _boundSectionId = section.Id;
                    }

                    var result = await _query.QueryAsync(section.Id, BuildFilter(includeSearch: true));
                    if (_reloadPending)
                        continue;

                    Summary = result.ActiveRiskSummary;
                    FilteredStudents.Clear();
                    _isUpdatingSelection = true;
                    foreach (var student in result.Students)
                    {
                        var row = new StudentRow(student, 0);
                        row.IsSelected = _selectedIds.Contains(student.Data.Student.Id);
                        row.PropertyChanged += (_, args) =>
                        {
                            if (args.PropertyName == nameof(StudentRow.IsSelected))
                                OnRowSelectionChanged(row);
                        };
                        FilteredStudents.Add(row);
                    }
                    _isUpdatingSelection = false;
                }
                catch (Exception ex)
                {
                    ErrorMessage = "No se pudieron cargar los estudiantes.";
                    System.Diagnostics.Debug.WriteLine(ex);
                    FilteredStudents.Clear();
                    Summary = RiskSummary.Empty;
                }

                NotifyListState();
                NotifySelectionChanged();
            }
            while (_reloadPending);
        }
        finally
        {
            IsBusy = false;
            NotifyListState();
        }
    }

    [RelayCommand]
    private void ToggleRisk(string level)
    {
        var target = level switch
        {
            "Alto" => RiskLevel.Alto,
            "Medio" => RiskLevel.Medio,
            "Bajo" => RiskLevel.Bajo,
            _ => (RiskLevel?)null
        };
        if (target is null)
            return;

        var onlyThis = target switch
        {
            RiskLevel.Alto => FilterRiskAlto && !FilterRiskMedio && !FilterRiskBajo,
            RiskLevel.Medio => FilterRiskMedio && !FilterRiskAlto && !FilterRiskBajo,
            _ => FilterRiskBajo && !FilterRiskAlto && !FilterRiskMedio
        };

        FilterRiskAlto = false;
        FilterRiskMedio = false;
        FilterRiskBajo = false;
        if (!onlyThis)
        {
            FilterRiskAlto = target == RiskLevel.Alto;
            FilterRiskMedio = target == RiskLevel.Medio;
            FilterRiskBajo = target == RiskLevel.Bajo;
        }
    }

    [RelayCommand]
    private void ToggleFilterColumn(string column) =>
        OpenFilterColumn = OpenFilterColumn == column ? string.Empty : column;

    [RelayCommand]
    private void SortByColumn(string column)
    {
        if (!Enum.TryParse<StudentsSortField>(column, out var field))
            return;

        if (SortBy == field)
            SortDescending = !SortDescending;
        else
        {
            SortBy = field;
            SortDescending = false;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        NameFilter = string.Empty;
        LegajoFilter = string.Empty;
        EmailFilter = string.Empty;
        AttendanceMinText = string.Empty;
        AttendanceMaxText = string.Empty;
        DeliveryMinText = string.Empty;
        DeliveryMaxText = string.Empty;
        FilterRiskAlto = false;
        FilterRiskMedio = false;
        FilterRiskBajo = false;
        FilterContactSin = false;
        FilterContactCon = false;
        ActiveFilterKey = "Activos";
        OpenFilterColumn = string.Empty;
        SortBy = StudentsSortField.Name;
        SortDescending = false;
    }

    [RelayCommand]
    private void ClearSelection()
    {
        _selectedIds.Clear();
        _isUpdatingSelection = true;
        foreach (var row in FilteredStudents)
            row.IsSelected = false;
        _isUpdatingSelection = false;
        NotifySelectionChanged();
    }

    [RelayCommand]
    private async Task OpenActionsAsync()
    {
        if (!HasSelection)
            return;

        ActionSearchText = string.Empty;
        ActionError = string.Empty;
        SelectedAction = null;
        ActionsStep = 0;
        HighlightedActionIndex = 0;
        RebuildActionCatalog();
        ApplyActionFilter();
        await LoadActionPickersAsync();
        IsActionsOpen = true;
    }

    [RelayCommand]
    private void CloseActions()
    {
        IsActionsOpen = false;
        ActionError = string.Empty;
        ActionsStep = 0;
        SelectedAction = null;
    }

    [RelayCommand]
    private void SelectAction(ActionPaletteItem? item)
    {
        if (item is null)
            return;

        SelectedAction = item;
        ActionError = string.Empty;
        ActionsStep = 1;
    }

    [RelayCommand]
    private void BackToActions()
    {
        ActionsStep = 0;
        ActionError = string.Empty;
        SelectedAction = null;
    }

    [RelayCommand]
    private void MoveActionHighlight(string direction)
    {
        if (FilteredActions.Count == 0)
            return;

        if (direction == "up")
            HighlightedActionIndex = (HighlightedActionIndex - 1 + FilteredActions.Count) % FilteredActions.Count;
        else
            HighlightedActionIndex = (HighlightedActionIndex + 1) % FilteredActions.Count;
    }

    [RelayCommand]
    private void ConfirmHighlightedAction()
    {
        if (FilteredActions.Count == 0)
            return;

        var index = Math.Clamp(HighlightedActionIndex, 0, FilteredActions.Count - 1);
        SelectAction(FilteredActions[index]);
    }

    [RelayCommand]
    private async Task ConfirmAttendanceAsync(string statusLabel)
    {
        if (SelectedClassSession is null)
        {
            ActionError = "Elegí una clase.";
            return;
        }

        if (!TryParseAttendance(statusLabel, out var status))
            return;

        await RunActionAsync(() => _attendance.SaveManyAsync(
            SelectedClassSession.Value.Id,
            _selectedIds.ToList(),
            status));
    }

    [RelayCommand]
    private async Task ConfirmSubmissionAsync(string statusLabel)
    {
        if (SelectedAssignment is null)
        {
            ActionError = "Elegí un trabajo.";
            return;
        }

        if (!TryParseSubmission(statusLabel, out var status))
            return;

        await RunActionAsync(() => _submissions.SaveManyAsync(
            SelectedAssignment.Value.Id,
            _selectedIds.ToList(),
            status));
    }

    [RelayCommand]
    private async Task ConfirmDeactivateAsync()
    {
        await RunActionAsync(async () =>
        {
            await _students.DeactivateAsync(_selectedIds.ToList());
            _selectedIds.Clear();
        });
    }

    [RelayCommand]
    private Task OpenAsync(StudentRow row) =>
        Shell.Current.GoToAsync(nameof(Views.StudentDetailPage), new Dictionary<string, object>
        {
            [StudentDetailViewModel.StudentQueryKey] = row,
            [StudentDetailViewModel.SectionQueryKey] = _selection.Current?.Name ?? string.Empty,
            [StudentDetailViewModel.SectionIdQueryKey] = _selection.Current?.Id ?? 0
        });

    [RelayCommand]
    private void ShowAdd()
    {
        LastName = string.Empty;
        FirstName = string.Empty;
        StudentNumber = string.Empty;
        Dni = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        EnrolledOn = DateTime.Today;
        AddError = string.Empty;
        IsAdding = true;
    }

    [RelayCommand]
    private void CancelAdd() => IsAdding = false;

    [RelayCommand]
    private async Task SaveStudentAsync()
    {
        var section = _selection.Current;
        if (section is null)
        {
            AddError = "Elegí una comisión.";
            return;
        }

        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            await _students.AddAsync(
                section.Id,
                new StudentDraft(LastName, FirstName, StudentNumber, Dni, EnrolledOn, Phone, Email),
                DateTime.Today);
            IsAdding = false;
            AddError = string.Empty;
            await LoadAsync();
        }
        catch (StudentEnrollmentException ex)
        {
            AddError = ex.Message;
        }
        catch (Exception ex)
        {
            AddError = "No se pudo guardar el estudiante.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RunActionAsync(Func<Task> action)
    {
        ActionError = string.Empty;
        try
        {
            await action();
            IsActionsOpen = false;
            StatusMessage = "Acción aplicada correctamente.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            ActionError = "No se pudo completar la acción.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async Task LoadActionPickersAsync()
    {
        ClassSessions.Clear();
        Assignments.Clear();
        SelectedClassSession = null;
        SelectedAssignment = null;
        var section = _selection.Current;
        if (section is null)
            return;

        var sessions = await _attendance.GetClassSessionsAsync(section.Id);
        foreach (var session in sessions)
            ClassSessions.Add(new PickerOption<ClassSession>(session, session.Date.ToString("dd/MM/yyyy")));
        SelectedClassSession = ClassSessions.FirstOrDefault(o =>
            o.Value == AttendanceService.PickDefault(sessions, DateTime.Today));

        var assignments = await _submissions.GetAssignmentsAsync(section.Id);
        foreach (var assignment in assignments)
        {
            var label = $"{assignment.Name} · {assignment.DueDate:dd/MM/yyyy}";
            Assignments.Add(new PickerOption<Assignment>(assignment, label));
        }

        SelectedAssignment = Assignments.FirstOrDefault(o =>
            o.Value == SubmissionService.PickDefault(assignments, DateTime.Today));
    }

    private void RebuildActionCatalog()
    {
        _allActions.Clear();
        _allActions.Add(new ActionPaletteItem(StudentsActionKind.Attendance, "Gestionar asistencias"));
        _allActions.Add(new ActionPaletteItem(StudentsActionKind.Submissions, "Gestionar entregas"));
        _allActions.Add(new ActionPaletteItem(StudentsActionKind.Deactivate, "Desactivar"));
    }

    private void ApplyActionFilter()
    {
        var query = ActionSearchText.Trim();
        FilteredActions.Clear();
        foreach (var action in _allActions)
        {
            if (query.Length == 0 || action.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                FilteredActions.Add(action);
        }

        HighlightedActionIndex = 0;
    }

    private void SetAllSelected(bool selected)
    {
        _isUpdatingSelection = true;
        foreach (var row in FilteredStudents)
        {
            row.IsSelected = selected;
            if (selected)
                _selectedIds.Add(row.Student.Id);
            else
                _selectedIds.Remove(row.Student.Id);
        }
        _isUpdatingSelection = false;
        NotifySelectionChanged();
    }

    private void OnRowSelectionChanged(StudentRow row)
    {
        if (_isUpdatingSelection)
            return;

        if (row.IsSelected)
            _selectedIds.Add(row.Student.Id);
        else
            _selectedIds.Remove(row.Student.Id);
        NotifySelectionChanged();
    }

    private void NotifySelectionChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(IsAllSelected));
        OnPropertyChanged(nameof(DeactivateConfirmText));
    }

    private void NotifyListState()
    {
        OnPropertyChanged(nameof(FilteredCount));
        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(ShowEmptyRoster));
        OnPropertyChanged(nameof(ShowSearchEmpty));
        OnPropertyChanged(nameof(ShowTable));
        OnPropertyChanged(nameof(HasActiveFilters));
        OnPropertyChanged(nameof(HighActive));
        OnPropertyChanged(nameof(MediumActive));
        OnPropertyChanged(nameof(LowActive));
    }

    private void NotifyFiltersAndReload()
    {
        NotifyListState();
        _ = ReloadQuietAsync();
    }

    private void NotifyRiskAndReload()
    {
        OnPropertyChanged(nameof(HighActive));
        OnPropertyChanged(nameof(MediumActive));
        OnPropertyChanged(nameof(LowActive));
        NotifyFiltersAndReload();
    }

    private async Task ReloadQuietAsync()
    {
        if (_selection.Current is null)
            return;
        await LoadCommand.ExecuteAsync(null);
    }

    private StudentsQueryFilter BuildFilter(bool includeSearch)
    {
        var risks = new List<RiskLevel>();
        if (FilterRiskAlto) risks.Add(RiskLevel.Alto);
        if (FilterRiskMedio) risks.Add(RiskLevel.Medio);
        if (FilterRiskBajo) risks.Add(RiskLevel.Bajo);

        var contacts = new List<ContactStatusFilter>();
        if (FilterContactSin) contacts.Add(ContactStatusFilter.SinContactar);
        if (FilterContactCon) contacts.Add(ContactStatusFilter.Contactado);

        return new StudentsQueryFilter
        {
            SearchText = includeSearch ? SearchText : null,
            NameContains = NullIfBlank(NameFilter),
            StudentNumberContains = NullIfBlank(LegajoFilter),
            EmailContains = NullIfBlank(EmailFilter),
            RiskLevels = risks.Count == 0 ? null : risks,
            AttendanceMinPercent = ParsePercent(AttendanceMinText),
            AttendanceMaxPercent = ParsePercent(AttendanceMaxText),
            DeliveryMinPercent = ParsePercent(DeliveryMinText),
            DeliveryMaxPercent = ParsePercent(DeliveryMaxText),
            ContactStatuses = contacts.Count == 0 ? null : contacts,
            ActiveStatus = ActiveFilterKey switch
            {
                "Inactivos" => ActiveStatusFilter.InactiveOnly,
                "Todos" => ActiveStatusFilter.All,
                _ => ActiveStatusFilter.ActiveOnly
            },
            SortBy = SortBy,
            SortDescending = SortDescending
        };
    }

    private static bool HasColumnFilters(StudentsQueryFilter filter) =>
        !string.IsNullOrWhiteSpace(filter.NameContains)
        || !string.IsNullOrWhiteSpace(filter.StudentNumberContains)
        || !string.IsNullOrWhiteSpace(filter.EmailContains)
        || filter.RiskLevels is { Count: > 0 }
        || filter.AttendanceMinPercent is not null
        || filter.AttendanceMaxPercent is not null
        || filter.DeliveryMinPercent is not null
        || filter.DeliveryMaxPercent is not null
        || filter.ContactStatuses is { Count: > 0 }
        || filter.ActiveStatus != ActiveStatusFilter.ActiveOnly;

    private static string? NullIfBlank(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParsePercent(string text) =>
        int.TryParse(text.Trim(), out var value) ? Math.Clamp(value, 0, 100) : null;

    private static bool TryParseAttendance(string label, out AttendanceStatus status)
    {
        status = label switch
        {
            "Presente" => AttendanceStatus.Presente,
            "Ausente" => AttendanceStatus.Ausente,
            _ => default
        };
        return label is "Presente" or "Ausente";
    }

    private static bool TryParseSubmission(string label, out SubmissionStatus status)
    {
        status = label switch
        {
            "Entregó" => SubmissionStatus.Entrego,
            "No entregó" => SubmissionStatus.NoEntrego,
            _ => default
        };
        return label is "Entregó" or "No entregó";
    }
}

public enum StudentsActionKind
{
    Attendance,
    Submissions,
    Deactivate
}

public sealed record ActionPaletteItem(StudentsActionKind Kind, string Title);
