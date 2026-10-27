using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels
{
    // Registrar entregas: comisión -> trabajo -> Entregó/No entregó por alumno.
    public partial class SubmissionsViewModel : SectionScopedViewModel
    {
        private static readonly IReadOnlyDictionary<string, SubmissionStatus> StatusByLabel =
            new Dictionary<string, SubmissionStatus>
            {
                ["Entregó"] = SubmissionStatus.Entrego,
                ["No entregó"] = SubmissionStatus.NoEntrego
            };

        private readonly SubmissionService _service;
        private bool _suppressAssignmentReload;

        public SubmissionsViewModel(SectionSelection selection, SubmissionService service) : base(selection)
        {
            _service = service;
        }

        public IReadOnlyList<string> StatusOptions { get; } = StatusByLabel.Keys.ToList();

        public ObservableCollection<PickerOption<Assignment>> Assignments { get; } = new();

        [ObservableProperty]
        public partial PickerOption<Assignment>? SelectedAssignment { get; set; }

        public ObservableCollection<StatusRow> Rows { get; } = new();

        public int RegisteredCount => Rows.Count(row => !string.IsNullOrEmpty(row.SelectedStatus));

        public int RowCount => Rows.Count;

        public double RegisteredProgress => RowCount == 0 ? 0 : (double)RegisteredCount / RowCount;

        public string RegisteredText => $"{RegisteredCount} de {RowCount} registrados";

        [ObservableProperty]
        public partial bool HasNoAssignments { get; set; }

        protected override async Task OnSectionChangedAsync()
        {
            Assignments.Clear();
            Rows.Clear();
            if (SelectedSection is null)
                return;

            var assignments = await _service.GetAssignmentsAsync(SelectedSection.Id);
            foreach (var a in assignments)
                Assignments.Add(new PickerOption<Assignment>(a, $"{a.Name} · {FormatDate(a.DueDate)}"));
            HasNoAssignments = Assignments.Count == 0;

            var def = SubmissionService.PickDefault(assignments, DateTime.Today);
            _suppressAssignmentReload = true;
            SelectedAssignment = Assignments.FirstOrDefault(o => o.Value == def);
            _suppressAssignmentReload = false;

            await LoadRowsAsync();
        }

        partial void OnSelectedAssignmentChanged(PickerOption<Assignment>? value)
        {
            if (_suppressAssignmentReload || value is null)
                return;

            _ = RunAsync(LoadRowsAsync);
        }

        private async Task LoadRowsAsync()
        {
            Rows.Clear();
            StatusMessage = string.Empty;
            if (SelectedAssignment is null)
                return;

            var marks = await _service.GetMarksAsync(SelectedAssignment.Value.Id);
            foreach (var m in marks)
                Rows.Add(Watch(new StatusRow(m.StudentId, m.StudentName, Label(m.Status), SaveAsync)));

            NotifyProgress();
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
            if (SelectedAssignment is null || !StatusByLabel.TryGetValue(label, out var status))
                return;

            try
            {
                await _service.SaveAsync(SelectedAssignment.Value.Id, row.StudentId, status);
                ErrorMessage = string.Empty;
                StatusMessage = $"Guardado: {row.Name} · {label}";
            }
            catch (Exception ex)
            {
                ReportError($"No se pudo guardar la entrega de {row.Name}.", ex);
            }
        }

        private static string? Label(SubmissionStatus? status) =>
            status is null ? null : StatusByLabel.First(kv => kv.Value == status).Key;
    }
}
