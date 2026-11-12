using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Formatting;
using UI.Services;

namespace UI.ViewModels;

// Paleta de acciones masivas sobre los alumnos tildados: elegir acción, clase o trabajo, y confirmar.
public sealed partial class StudentsActionsViewModel : ObservableObject
{
    // Una sola lista de definiciones: agregar una acción es agregar una fila acá.
    private static readonly ActionPaletteItem[] Catalog =
    [
        new(StudentsActionKind.Attendance, "Gestionar asistencias"),
        new(StudentsActionKind.Submissions, "Gestionar entregas"),
        new(StudentsActionKind.Deactivate, "Desactivar", count =>
            $"Vas a desactivar {count} {(count == 1 ? "estudiante" : "estudiantes")}. " +
            "Quedarán ocultos por defecto y se podrán volver a ver con el filtro de Estado.")
    ];

    private readonly IAttendanceService _attendance;
    private readonly ISubmissionService _submissions;
    private readonly IStudentService _students;
    private readonly SectionSelection _selection;
    private readonly StudentsSelectionViewModel _rowSelection;
    private readonly IClock _clock;
    private readonly Func<Task> _applied;

    public StudentsActionsViewModel(
        IAttendanceService attendance,
        ISubmissionService submissions,
        IStudentService students,
        SectionSelection selection,
        StudentsSelectionViewModel rowSelection,
        IClock clock,
        Func<Task> applied)
    {
        _attendance = attendance;
        _submissions = submissions;
        _students = students;
        _selection = selection;
        _rowSelection = rowSelection;
        _clock = clock;
        _applied = applied;
        _rowSelection.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(StudentsSelectionViewModel.SelectedCount))
                OnPropertyChanged(nameof(DeactivateConfirmText));
        };
    }

    public ObservableCollection<ActionPaletteItem> FilteredActions { get; } = new();

    public ObservableCollection<PickerOption<ClassSession>> ClassSessions { get; } = new();

    public ObservableCollection<PickerOption<Assignment>> Assignments { get; } = new();

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

    public bool HasActionError => !string.IsNullOrEmpty(ActionError);
    public bool IsActionsStep1 => ActionsStep == 0;
    public bool IsActionsStep2 => ActionsStep == 1;
    public bool IsAttendanceAction => IsKind(StudentsActionKind.Attendance);
    public bool IsSubmissionsAction => IsKind(StudentsActionKind.Submissions);
    public bool IsDeactivateAction => IsKind(StudentsActionKind.Deactivate);

    public string DeactivateConfirmText =>
        SelectedAction?.ConfirmText?.Invoke(_rowSelection.SelectedCount) ?? string.Empty;

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
    private async Task OpenActionsAsync()
    {
        if (!_rowSelection.HasSelection)
            return;

        ActionSearchText = string.Empty;
        ActionError = string.Empty;
        SelectedAction = null;
        ActionsStep = 0;
        HighlightedActionIndex = 0;
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

        if (!StatusLabels.TryParseAttendance(statusLabel, out var status))
            return;

        await RunActionAsync(() => _attendance.SaveManyAsync(
            SelectedClassSession.Value.Id,
            _rowSelection.Ids,
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

        if (!StatusLabels.TryParseSubmission(statusLabel, out var status))
            return;

        await RunActionAsync(() => _submissions.SaveManyAsync(
            SelectedAssignment.Value.Id,
            _rowSelection.Ids,
            status));
    }

    [RelayCommand]
    private async Task ConfirmDeactivateAsync()
    {
        await RunActionAsync(async () =>
        {
            await _students.DeactivateAsync(_rowSelection.Ids);
            _rowSelection.Reset();
        });
    }

    private bool IsKind(StudentsActionKind kind) => SelectedAction?.Kind == kind;

    private async Task RunActionAsync(Func<Task> action)
    {
        ActionError = string.Empty;
        try
        {
            await action();
            IsActionsOpen = false;
            await _applied();
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
        if (_selection.Current is not { } section)
            return;

        var today = _clock.Today;
        var sessions = await _attendance.GetClassSessionsAsync(section.Id);
        foreach (var session in sessions)
            ClassSessions.Add(new PickerOption<ClassSession>(session, session.Date.ToString("dd/MM/yyyy")));
        SelectedClassSession = ClassSessions.FirstOrDefault(o =>
            o.Value == AttendanceService.PickDefault(sessions, today));

        var assignments = await _submissions.GetAssignmentsAsync(section.Id);
        foreach (var assignment in assignments)
            Assignments.Add(new PickerOption<Assignment>(assignment, $"{assignment.Name} · {assignment.DueDate:dd/MM/yyyy}"));
        SelectedAssignment = Assignments.FirstOrDefault(o =>
            o.Value == SubmissionService.PickDefault(assignments, today));
    }

    private void ApplyActionFilter()
    {
        var query = ActionSearchText.Trim();
        FilteredActions.Clear();
        foreach (var action in Catalog)
        {
            if (query.Length == 0 || action.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                FilteredActions.Add(action);
        }

        HighlightedActionIndex = 0;
    }
}
