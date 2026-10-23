using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

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

        public SubmissionsViewModel(SectionService sectionService, SubmissionService service) : base(sectionService)
        {
            _service = service;
        }

        public IReadOnlyList<string> StatusOptions { get; } = StatusByLabel.Keys.ToList();

        public ObservableCollection<PickerOption<Assignment>> Assignments { get; } = new();

        [ObservableProperty]
        public partial PickerOption<Assignment>? SelectedAssignment { get; set; }

        public ObservableCollection<StatusRow> Rows { get; } = new();

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
                Rows.Add(new StatusRow(m.StudentId, m.StudentName, Label(m.Status), SaveAsync));
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
