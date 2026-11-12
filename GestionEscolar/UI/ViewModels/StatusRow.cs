using CommunityToolkit.Mvvm.ComponentModel;
using UI.Formatting;

namespace UI.ViewModels
{
    // Fila de alumno con estado y selección para Asistencia y Entregas.
    public sealed partial class StatusRow : ObservableObject
    {
        private readonly Action<StatusRow>? _onSelectionChanged;
        private readonly Func<StatusRow, string, Task>? _onChanged;

        public StatusRow(int studentId, string name, string? status, Action<StatusRow>? onSelectionChanged = null)
        {
            StudentId = studentId;
            Name = name;
            SelectedStatus = status;
            _onSelectionChanged = onSelectionChanged;
        }

        public StatusRow(int studentId, string name, string? status, Func<StatusRow, string, Task>? onChanged)
        {
            StudentId = studentId;
            Name = name;
            SelectedStatus = status;
            _onChanged = onChanged;
        }

        public int StudentId { get; }

        public string Name { get; }

        public string DisplayName => PersonName.AsSurnameFirst(Name);

        public string Initials => PersonName.Initials(DisplayName);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusDisplay), nameof(StatusKind))]
        public partial string? SelectedStatus { get; set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        public string StatusDisplay => string.IsNullOrEmpty(SelectedStatus) ? StatusLabels.NoRecord : SelectedStatus;

        public string StatusKind => StatusLabels.Kind(SelectedStatus);

        partial void OnSelectedStatusChanged(string? oldValue, string? newValue)
        {
            if (_onChanged is null || newValue is null || newValue == oldValue)
                return;

            _ = _onChanged(this, newValue);
        }

        partial void OnIsSelectedChanged(bool value)
        {
            _onSelectionChanged?.Invoke(this);
        }
    }
}
