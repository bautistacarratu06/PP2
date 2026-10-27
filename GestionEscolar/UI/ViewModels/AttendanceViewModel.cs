using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels
{
    // Registrar asistencia: comisión -> clase (fecha) -> Presente/Ausente por alumno.
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

        public AttendanceViewModel(SectionSelection selection, AttendanceService service) : base(selection)
        {
            _service = service;
        }

        public IReadOnlyList<string> StatusOptions { get; } = StatusByLabel.Keys.ToList();

        public ObservableCollection<PickerOption<ClassSession>> ClassSessions { get; } = new();

        [ObservableProperty]
        public partial PickerOption<ClassSession>? SelectedClassSession { get; set; }

        public ObservableCollection<StatusRow> Rows { get; } = new();

        public int RegisteredCount => Rows.Count(row => !string.IsNullOrEmpty(row.SelectedStatus));

        public int RowCount => Rows.Count;

        public double RegisteredProgress => RowCount == 0 ? 0 : (double)RegisteredCount / RowCount;

        public string RegisteredText => $"{RegisteredCount} de {RowCount} registrados";

        [ObservableProperty]
        public partial bool HasNoClasses { get; set; }

        protected override async Task OnSectionChangedAsync()
        {
            ClassSessions.Clear();
            Rows.Clear();
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
            StatusMessage = string.Empty;
            if (SelectedClassSession is null)
                return;

            var marks = await _service.GetMarksAsync(SelectedClassSession.Value.Id);
            foreach (var m in marks)
                Rows.Add(Watch(new StatusRow(m.StudentId, m.StudentName, Label(m.Status), SaveAsync)));

            NotifyProgress();
        }

        [RelayCommand]
        private void MarkAllPresent()
        {
            foreach (var row in Rows)
            {
                if (row.SelectedStatus != "Presente")
                    row.SelectedStatus = "Presente";
            }
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

        private void NotifyProgress()
        {
            OnPropertyChanged(nameof(RegisteredCount));
            OnPropertyChanged(nameof(RowCount));
            OnPropertyChanged(nameof(RegisteredProgress));
            OnPropertyChanged(nameof(RegisteredText));
        }

        private async Task SaveAsync(StatusRow row, string label)
        {
            if (SelectedClassSession is null || !StatusByLabel.TryGetValue(label, out var status))
                return;

            try
            {
                await _service.SaveAsync(SelectedClassSession.Value.Id, row.StudentId, status);
                ErrorMessage = string.Empty;
                StatusMessage = $"Guardado: {row.Name} · {label}";
            }
            catch (Exception ex)
            {
                ReportError($"No se pudo guardar la asistencia de {row.Name}.", ex);
            }
        }

        private static string? Label(AttendanceStatus? status) =>
            status is null ? null : StatusByLabel.First(kv => kv.Value == status).Key;
    }
}
