using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels;

// Alumnos tildados en la grilla: ids elegidos, "todos" y el texto del contador.
public sealed partial class StudentsSelectionViewModel : ObservableObject
{
    private readonly HashSet<int> _ids = [];
    private readonly ObservableCollection<StudentRow> _rows;
    private bool _isUpdating;

    public StudentsSelectionViewModel(ObservableCollection<StudentRow> rows)
    {
        _rows = rows;
    }

    public IReadOnlyList<int> Ids => _ids.ToList();

    public int SelectedCount => _ids.Count;

    public bool HasSelection => SelectedCount > 0;

    public string SelectedCountText =>
        $"{SelectedCount} {(SelectedCount == 1 ? "estudiante seleccionado" : "estudiantes seleccionados")}";

    public bool IsAllSelected
    {
        get => _rows.Count > 0 && _rows.All(row => row.IsSelected);
        set => SetAllSelected(value);
    }

    // Reemplaza las filas de la grilla conservando lo que ya estaba tildado.
    public void ReplaceRows(IEnumerable<StudentRow> rows)
    {
        _rows.Clear();
        _isUpdating = true;
        foreach (var row in rows)
        {
            row.IsSelected = _ids.Contains(row.Student.Id);
            row.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(StudentRow.IsSelected))
                    OnRowSelectionChanged(row);
            };
            _rows.Add(row);
        }

        _isUpdating = false;
        NotifyChanged();
    }

    public void ClearRows()
    {
        _rows.Clear();
        NotifyChanged();
    }

    public void Reset()
    {
        _ids.Clear();
        NotifyChanged();
    }

    [RelayCommand]
    private void Clear()
    {
        _ids.Clear();
        _isUpdating = true;
        foreach (var row in _rows)
            row.IsSelected = false;
        _isUpdating = false;
        NotifyChanged();
    }

    private void SetAllSelected(bool selected)
    {
        _isUpdating = true;
        foreach (var row in _rows)
        {
            row.IsSelected = selected;
            if (selected)
                _ids.Add(row.Student.Id);
            else
                _ids.Remove(row.Student.Id);
        }

        _isUpdating = false;
        NotifyChanged();
    }

    private void OnRowSelectionChanged(StudentRow row)
    {
        if (_isUpdating)
            return;

        if (row.IsSelected)
            _ids.Add(row.Student.Id);
        else
            _ids.Remove(row.Student.Id);
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(SelectedCountText));
        OnPropertyChanged(nameof(IsAllSelected));
    }
}
