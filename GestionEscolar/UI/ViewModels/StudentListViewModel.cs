using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

public partial class StudentListViewModel : ObservableObject
{
    private readonly StudentService _students;
    private readonly SectionSelection _selection;
    private readonly List<StudentRow> _all = [];

    public StudentListViewModel(StudentService students, SectionSelection selection)
    {
        _students = students;
        _selection = selection;
        _selection.Changed += (_, _) => MainThread.BeginInvokeOnMainThread(() => _ = LoadCommand.ExecuteAsync(null));
    }

    public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

    public int FilteredCount => FilteredStudents.Count;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

    public bool HasNoResults => !IsBusy && FilteredStudents.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public string EmptyText =>
        string.IsNullOrWhiteSpace(SearchText)
            ? "No hay estudiantes para mostrar."
            : "Sin resultados para esa búsqueda.";

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _selection.EnsureLoadedAsync();
            var section = _selection.Current;
            _all.Clear();
            if (section is not null)
            {
                var students = await _students.GetRosterAsync(section.Id);
                foreach (var student in students)
                    _all.Add(new StudentRow(student));
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar los estudiantes.";
            System.Diagnostics.Debug.WriteLine($"No se pudieron cargar los estudiantes: {ex}");
            _all.Clear();
        }
        finally
        {
            IsBusy = false;
            ApplyFilter();
        }
    }

    [RelayCommand]
    private Task OpenAsync(StudentRow row) =>
        Shell.Current.GoToAsync(nameof(Views.StudentDetailPage), new Dictionary<string, object>
        {
            [StudentDetailViewModel.StudentQueryKey] = row,
            [StudentDetailViewModel.SectionQueryKey] = _selection.Current?.Name ?? string.Empty,
            [StudentDetailViewModel.SectionIdQueryKey] = _selection.Current?.Id ?? 0
        });

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        FilteredStudents.Clear();
        foreach (var row in _all)
        {
            if (query.Length == 0
                || row.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || row.Student.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || row.StudentNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                FilteredStudents.Add(row);
            }
        }

        OnPropertyChanged(nameof(FilteredCount));
        OnPropertyChanged(nameof(HasNoResults));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(EmptyText));
    }
}
