using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;

namespace UI.Services;

// Resultado de una importación en segundo plano: el mensaje final y si quedaron filas con error.
public sealed record ImportJobResult(string Message, bool HasRowErrors);

// Importación de comisión que corre fuera del hilo de la UI. Es singleton: el aviso de avance
// sigue vivo aunque la persona cambie de pantalla y vuelva. Solo corre una a la vez.
public sealed partial class ImportJobTracker : ObservableObject
{
    private static readonly TimeSpan AutoHideDelay = TimeSpan.FromSeconds(6);

    private int _runId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle), nameof(IsFinished))]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StageText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CountText { get; set; } = string.Empty;

    // 0 a 1 para la barra; con IsIndeterminate se muestra un indicador sin porcentaje.
    [ObservableProperty]
    public partial double Progress { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDeterminate))]
    public partial bool IsIndeterminate { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResult))]
    public partial string ResultText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsError { get; set; }

    public bool IsIdle => !IsRunning;

    public bool IsFinished => !IsRunning && IsVisible;

    public bool IsDeterminate => !IsIndeterminate;

    public bool HasResult => !string.IsNullOrEmpty(ResultText);

    // Devuelve el resultado, o null si la importación falló (el error queda en el aviso).
    public async Task<ImportJobResult?> RunAsync(string title, Func<IProgress<ImportProgress>, Task<ImportJobResult>> work)
    {
        ArgumentNullException.ThrowIfNull(work);
        if (IsRunning)
            return null;

        var runId = ++_runId;
        Start(title);
        // Se crea en el hilo de la UI: cada Report vuelve a ese hilo solo.
        var progress = new Progress<ImportProgress>(Show);
        ImportJobResult? result = null;
        try
        {
            result = await Task.Run(() => work(progress));
            Finish(result.Message, isError: result.HasRowErrors);
        }
        catch (ImportValidationException ex)
        {
            Finish(ex.Message, isError: true);
        }
        catch (Exception ex)
        {
            Finish("No se pudo completar la importación.", isError: true);
            System.Diagnostics.Debug.WriteLine(ex);
        }

        if (!IsError)
            _ = HideLaterAsync(runId);
        return result;
    }

    [RelayCommand]
    private void Dismiss()
    {
        if (!IsRunning)
            IsVisible = false;
    }

    private void Start(string title)
    {
        Title = title;
        ResultText = string.Empty;
        IsError = false;
        IsRunning = true;
        IsVisible = true;
        Show(new ImportProgress(ImportStage.Checking, 0, 0));
    }

    private void Finish(string message, bool isError)
    {
        IsRunning = false;
        IsError = isError;
        ResultText = message;
        StageText = isError ? "Importación con problemas" : "Importación terminada";
        CountText = string.Empty;
        IsIndeterminate = false;
        Progress = isError ? Progress : 1;
        OnPropertyChanged(nameof(IsFinished));
    }

    private void Show(ImportProgress value)
    {
        if (!IsRunning)
            return;

        StageText = value.Stage switch
        {
            ImportStage.Reading => "Leyendo el archivo",
            ImportStage.Checking => "Revisando DNI y legajos",
            ImportStage.Saving => "Guardando alumnos",
            _ => "Confirmando en la base"
        };
        IsIndeterminate = value.Total <= 0 || value.Stage == ImportStage.Finishing;
        Progress = value.Total > 0 ? Math.Clamp((double)value.Done / value.Total, 0, 1) : 0;
        CountText = Count(value);
    }

    private static string Count(ImportProgress value)
    {
        var culture = CultureInfo.CurrentCulture;
        if (value.Stage == ImportStage.Finishing)
            return value.Total > 0 ? string.Format(culture, "{0:N0} alumnos", value.Total) : string.Empty;
        if (value.Total > 0)
            return string.Format(culture, "{0:N0} de {1:N0}", value.Done, value.Total);
        return value.Done > 0 ? string.Format(culture, "{0:N0} filas", value.Done) : string.Empty;
    }

    private async Task HideLaterAsync(int runId)
    {
        await Task.Delay(AutoHideDelay);
        if (runId == _runId && !IsRunning && !IsError)
            IsVisible = false;
    }
}
