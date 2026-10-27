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

    public StudentListViewModel(StudentService students, SectionSelection selection, CommissionImportViewModel import)
    {
        _students = students;
        _selection = selection;
        Import = import;
        _selection.Changed += (_, _) => MainThread.BeginInvokeOnMainThread(() => _ = LoadCommand.ExecuteAsync(null));
    }

    public CommissionImportViewModel Import { get; }

    public DateTime Today => DateTime.Today;

    public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

    public int FilteredCount => FilteredStudents.Count;

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string ErrorMessage { get; set; } = string.Empty;

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

    public bool HasNoResults => !IsBusy && FilteredStudents.Count == 0 && string.IsNullOrEmpty(ErrorMessage);

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool HasAddError => !string.IsNullOrEmpty(AddError);

    public bool ShowEmptyRoster => HasNoResults && string.IsNullOrWhiteSpace(SearchText);

    public bool ShowSearchEmpty => HasNoResults && !string.IsNullOrWhiteSpace(SearchText);

    public bool ShowTable => !ShowEmptyRoster && string.IsNullOrEmpty(ErrorMessage);

    public string EmptyText => "Sin resultados para esa búsqueda.";

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
        OnPropertyChanged(nameof(ShowEmptyRoster));
        OnPropertyChanged(nameof(ShowSearchEmpty));
        OnPropertyChanged(nameof(ShowTable));
    }

    partial void OnAddErrorChanged(string value) => OnPropertyChanged(nameof(HasAddError));

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
    }
}
