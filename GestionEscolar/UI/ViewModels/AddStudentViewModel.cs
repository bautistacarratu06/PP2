using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

// Formulario "Nuevo estudiante" de la pantalla Estudiantes.
public sealed partial class AddStudentViewModel : ObservableObject
{
    private readonly IStudentService _students;
    private readonly SectionSelection _selection;
    private readonly IClock _clock;
    private readonly Func<Task> _reload;
    private bool _isSaving;

    public AddStudentViewModel(IStudentService students, SectionSelection selection, IClock clock, Func<Task> reload)
    {
        _students = students;
        _selection = selection;
        _clock = clock;
        _reload = reload;
        EnrolledOn = clock.Today;
    }

    public DateTime Today => _clock.Today;

    public bool HasAddError => !string.IsNullOrEmpty(AddError);

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
    public partial DateTime EnrolledOn { get; set; }

    [ObservableProperty]
    public partial string Phone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AddError { get; set; } = string.Empty;

    partial void OnAddErrorChanged(string value) => OnPropertyChanged(nameof(HasAddError));

    [RelayCommand]
    private void Show()
    {
        LastName = string.Empty;
        FirstName = string.Empty;
        StudentNumber = string.Empty;
        Dni = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        EnrolledOn = _clock.Today;
        AddError = string.Empty;
        IsAdding = true;
    }

    [RelayCommand]
    private void Cancel() => IsAdding = false;

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_selection.Current is not { } section)
        {
            AddError = "Elegí una comisión.";
            return;
        }

        if (_isSaving)
            return;

        _isSaving = true;
        try
        {
            var draft = new StudentDraft(LastName, FirstName, StudentNumber, Dni, EnrolledOn, Phone, Email);
            await _students.AddAsync(section.Id, draft, _clock.Today);
            IsAdding = false;
            AddError = string.Empty;
            await _reload();
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
        finally
        {
            _isSaving = false;
        }
    }
}
