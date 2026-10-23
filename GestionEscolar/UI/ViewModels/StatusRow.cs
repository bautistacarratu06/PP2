using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels
{
    // Fila "alumno + Picker de estado" para Asistencia y Entregas.
    // El cambio de SelectedStatus dispara el guardado (callback), salvo la asignación inicial.
    public sealed partial class StatusRow : ObservableObject
    {
        private readonly Func<StatusRow, string, Task>? _onChanged;

        public StatusRow(int studentId, string name, string? status, Func<StatusRow, string, Task> onChanged)
        {
            StudentId = studentId;
            Name = name;
            SelectedStatus = status; // antes de enganchar el callback: no guarda
            _onChanged = onChanged;
        }

        public int StudentId { get; }

        public string Name { get; }

        [ObservableProperty]
        public partial string? SelectedStatus { get; set; }

        partial void OnSelectedStatusChanged(string? oldValue, string? newValue)
        {
            if (_onChanged is null || newValue is null || newValue == oldValue)
                return;

            _ = _onChanged(this, newValue);
        }
    }
}
