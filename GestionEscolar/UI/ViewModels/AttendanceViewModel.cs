using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels
{
    // Registrar asistencia: comisión -> clase (fecha) -> Selección múltiple + Action Bar.
    public partial class AttendanceViewModel : SectionScopedViewModel
    {
        private static readonly IReadOnlyDictionary<string, AttendanceStatus> StatusByLabel =
            new Dictionary<string, AttendanceStatus>
            {
                ["Presente"] = AttendanceStatus.Presente,
                ["Ausente"] = AttendanceStatus.Ausente
            };

        private readonly AttendanceService _service;
        private bool _suppressSessionReload;
        private bool _isUpdatingSelection;
        private Func<Task>? _pendingBulkAction;

        public AttendanceViewModel(SectionSelection selection, AttendanceService service) : base(selection)
        {
            _service = service;
        }

        public bool CanPerformBulkActions => true;

        public IReadOnlyList<string> StatusOptions { get; } = StatusByLabel.Keys.ToList();

        public ObservableCollection<PickerOption<ClassSession>> ClassSessions { get; } = new();

        [ObservableProperty]
        public partial PickerOption<ClassSession>? SelectedClassSession { get; set; }

        public ObservableCollection<StatusRow> Rows { get; } = new();

        public int RegisteredCount => Rows.Count(row => !string.IsNullOrEmpty(row.SelectedStatus));

        public int RowCount => Rows.Count;

        public double RegisteredProgress => RowCount == 0 ? 0 : (double)RegisteredCount / RowCount;

        public string RegisteredText => $"{RegisteredCount} de {RowCount} registrados";

        public int SelectedCount => Rows.Count(row => row.IsSelected);

        public bool HasSelection => SelectedCount > 0;

        public string SelectedCountText => $"{SelectedCount} {(SelectedCount == 1 ? "estudiante seleccionado" : "estudiantes seleccionados")}";

        public bool IsAllSelected
        {
            get => Rows.Count > 0 && Rows.All(row => row.IsSelected);
            set => SetAllSelected(value);
        }

        [ObservableProperty]
        public partial bool HasNoClasses { get; set; }

        [ObservableProperty]
        public partial bool IsConfirmationOpen { get; set; }

        [ObservableProperty]
        public partial string ConfirmationTitle { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string ConfirmationMessage { get; set; } = string.Empty;

        protected override async Task OnSectionChangedAsync()
        {
            ClassSessions.Clear();
            HasNoClasses = false;
            Rows.Clear();
            NotifySelectionChanged();
            if (SelectedSection is null)
                return;

            var sessions = await _service.GetClassSessionsAsync(SelectedSection.Id);
            foreach (var s in sessions)
                ClassSessions.Add(new PickerOption<ClassSession>(s, FormatDate(s.Date)));
            HasNoClasses = ClassSessions.Count == 0;

            var def = AttendanceService.PickDefault(sessions, DateTime.Today);
            _suppressSessionReload = true;
            SelectedClassSession = ClassSessions.FirstOrDefault(o => o.Value == def);
            _suppressSessionReload = false;

            await LoadRowsAsync();
        }

        partial void OnSelectedClassSessionChanged(PickerOption<ClassSession>? value)
        {
            if (_suppressSessionReload || value is null)
                return;

            _ = RunAsync(LoadRowsAsync);
        }

        private async Task LoadRowsAsync()
        {
            Rows.Clear();
            NotifySelectionChanged();
            StatusMessage = string.Empty;
            if (SelectedClassSession is null)
                return;

            var marks = await _service.GetMarksAsync(SelectedClassSession.Value.Id);
            foreach (var m in marks)
                Rows.Add(Watch(new StatusRow(m.StudentId, m.StudentName, Label(m.Status), _ => NotifySelectionChanged())));

            NotifyProgress();
            NotifySelectionChanged();
        }

        [RelayCommand]
        private void SelectAll()
        {
            IsAllSelected = !IsAllSelected;
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SetAllSelected(false, force: true);
        }

        [RelayCommand]
        private void RequestMarkPresent()
        {
            if (SelectedCount == 0) return;
            ConfirmationTitle = "Marcar como Presente";
            ConfirmationMessage = $"Estás a punto de marcar como Presente a {SelectedCount} {(SelectedCount == 1 ? "estudiante" : "estudiantes")}.";
            _pendingBulkAction = () => ApplyBulkStatusAsync(AttendanceStatus.Presente);
            IsConfirmationOpen = true;
        }

        [RelayCommand]
        private void RequestMarkAbsent()
        {
            if (SelectedCount == 0) return;
            ConfirmationTitle = "Marcar como Ausente";
            ConfirmationMessage = $"Estás a punto de marcar como Ausente a {SelectedCount} {(SelectedCount == 1 ? "estudiante" : "estudiantes")}.";
            _pendingBulkAction = () => ApplyBulkStatusAsync(AttendanceStatus.Ausente);
            IsConfirmationOpen = true;
        }

        [RelayCommand]
        private async Task ConfirmActionAsync()
        {
            var action = _pendingBulkAction;
            _pendingBulkAction = null;
            IsConfirmationOpen = false;

            if (action is not null)
                await action();
        }

        [RelayCommand]
        private void CancelConfirmation()
        {
            _pendingBulkAction = null;
            IsConfirmationOpen = false;
        }

        [RelayCommand]
        private void MarkAllPresent()
        {
            SetAllSelected(true);
            RequestMarkPresent();
        }

        private async Task ApplyBulkStatusAsync(AttendanceStatus status)
        {
            if (SelectedClassSession is null)
                return;

            var targets = Rows.Where(r => r.IsSelected).ToList();
            if (targets.Count == 0)
                return;

            var label = Label(status);
            IsBusy = true;
            _isUpdatingSelection = true;
            try
            {
                foreach (var row in targets)
                {
                    await _service.SaveAsync(SelectedClassSession.Value.Id, row.StudentId, status);
                    row.SelectedStatus = label;
                    row.IsSelected = false;
                }

                StatusMessage = $"Se actualizaron {targets.Count} registros a {label}.";
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ReportError("No se pudieron guardar algunos registros de asistencia.", ex);
            }
            finally
            {
                _isUpdatingSelection = false;
                IsBusy = false;
            }

            NotifyProgress();
            NotifySelectionChanged();
        }

        private StatusRow Watch(StatusRow row)
        {
            row.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(StatusRow.SelectedStatus))
                    NotifyProgress();
            };
            return row;
        }

        private void SetAllSelected(bool value, bool force = false)
        {
            if (Rows.Count == 0 || _isUpdatingSelection)
                return;

            // Evita que el TwoWay del checkbox maestro reescriba el lote
            // cuando el valor ya coincide con el estado real de las filas.
            if (!force && value == Rows.All(row => row.IsSelected))
                return;

            _isUpdatingSelection = true;
            try
            {
                foreach (var row in Rows)
                    row.IsSelected = value;
            }
            finally
            {
                _isUpdatingSelection = false;
            }

            NotifySelectionChanged();
        }

        private void NotifyProgress()
        {
            OnPropertyChanged(nameof(RegisteredCount));
            OnPropertyChanged(nameof(RowCount));
            OnPropertyChanged(nameof(RegisteredProgress));
            OnPropertyChanged(nameof(RegisteredText));
        }

        private void NotifySelectionChanged()
        {
            if (_isUpdatingSelection)
                return;

            OnPropertyChanged(nameof(SelectedCount));
            OnPropertyChanged(nameof(HasSelection));
            OnPropertyChanged(nameof(SelectedCountText));
            OnPropertyChanged(nameof(IsAllSelected));
        }

        private static string? Label(AttendanceStatus? status) =>
            status is null ? null : StatusByLabel.First(kv => kv.Value == status).Key;
    }
}
