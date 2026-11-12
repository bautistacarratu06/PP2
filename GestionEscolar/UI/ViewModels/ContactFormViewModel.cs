using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

// Formulario "Registrar contacto" del detalle del alumno.
public sealed partial class ContactFormViewModel : ObservableObject
{
    private readonly IStudentDetailsService _details;
    private readonly IClock _clock;
    private readonly Func<int> _studentId;
    private readonly Func<Task> _reload;
    private bool _isSaving;

    public ContactFormViewModel(IStudentDetailsService details, IClock clock, Func<int> studentId, Func<Task> reload)
    {
        _details = details;
        _clock = clock;
        _studentId = studentId;
        _reload = reload;
        ContactDate = clock.Today;
    }

    public DateTime Today => _clock.Today;

    public IReadOnlyList<string> ChannelOptions { get; } =
        Enum.GetValues<ContactChannel>().Select(channel => channel.ToDbValue()).ToList();

    public IReadOnlyList<string> OutcomeOptions { get; } =
        Enum.GetValues<ContactOutcome>().Select(outcome => outcome.ToDbValue()).ToList();

    public bool HasContactError => !string.IsNullOrEmpty(ContactError);

    [ObservableProperty]
    public partial bool IsContactOpen { get; set; }

    [ObservableProperty]
    public partial DateTime ContactDate { get; set; }

    [ObservableProperty]
    public partial string? SelectedChannel { get; set; }

    [ObservableProperty]
    public partial string? SelectedOutcome { get; set; }

    [ObservableProperty]
    public partial string Note { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasContactError))]
    public partial string ContactError { get; set; } = string.Empty;

    [RelayCommand]
    private void OpenContact()
    {
        ContactDate = _clock.Today;
        SelectedChannel = null;
        SelectedOutcome = null;
        Note = string.Empty;
        ContactError = string.Empty;
        IsContactOpen = true;
    }

    [RelayCommand]
    private void CancelContact() => IsContactOpen = false;

    [RelayCommand]
    private async Task SaveContactAsync()
    {
        var studentId = _studentId();
        if (studentId <= 0 || _isSaving)
            return;

        _isSaving = true;
        try
        {
            await RegisterAsync(studentId);
            IsContactOpen = false;
            ContactError = string.Empty;
            await _reload();
        }
        catch (ContactValidationException ex)
        {
            ContactError = ex.Message;
        }
        catch (ArgumentOutOfRangeException)
        {
            ContactError = "Elegí un medio y un resultado.";
        }
        catch (Exception ex)
        {
            ContactError = "No se pudo guardar el contacto.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private Task RegisterAsync(int studentId) =>
        _details.RegisterContactAsync(
            studentId,
            ContactDate,
            DbStatusValues.ParseContactChannel(SelectedChannel),
            DbStatusValues.ParseContactOutcome(SelectedOutcome),
            Note,
            _clock.Today);
}
