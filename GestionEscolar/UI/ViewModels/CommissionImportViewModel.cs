using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

public partial class CommissionImportViewModel : ObservableObject
{
    private readonly ICommissionImportService _import;
    private readonly SectionSelection _selection;
    private readonly CommissionImportFileStep _file;
    private readonly IClock _clock;
    private readonly ImportJobTracker _jobs;

    // Tarjetas de conflicto que se dibujan; el resto queda en _choices con "Omitir".
    private const int MaxConflictCards = 50;

    private IReadOnlyList<RosterCsvRow> _rows = [];
    private List<ConflictChoice> _choices = [];
    private ImportPreview? _preview;

    public CommissionImportViewModel(
        ICommissionImportService import,
        SectionSelection selection,
        CommissionImportFileStep file,
        IClock clock,
        ImportJobTracker jobs)
    {
        _import = import;
        _selection = selection;
        _file = file;
        _clock = clock;
        _jobs = jobs;
    }

    public ObservableCollection<Section> Sections { get; } = new();

    public ObservableCollection<ConflictChoice> Conflicts { get; } = new();

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    public event Action<string>? Imported;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFileStep), nameof(IsDestinationStep), nameof(IsPreviewStep), nameof(IsNotPreviewStep), nameof(NextLabel))]
    public partial int Step { get; set; }

    [ObservableProperty]
    public partial string ChosenFileName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial string ErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChooseExisting))]
    public partial bool CreateNew { get; set; } = true;

    [ObservableProperty]
    public partial string NewName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Section? SelectedSection { get; set; }

    [ObservableProperty]
    public partial string InsertText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasBlockedText))]
    public partial string BlockedText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    // Qué está haciendo el asistente mientras IsBusy (leer el archivo, revisar conflictos).
    [ObservableProperty]
    public partial string BusyText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMoreConflicts))]
    public partial string MoreConflictsText { get; set; } = string.Empty;

    public bool HasMoreConflicts => !string.IsNullOrEmpty(MoreConflictsText);

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsFileStep => Step == 0;

    public bool IsDestinationStep => Step == 1;

    public bool IsPreviewStep => Step == 2;

    public bool IsNotPreviewStep => Step != 2;

    public bool HasBlockedText => !string.IsNullOrEmpty(BlockedText);

    public bool HasConflicts => Conflicts.Count > 0;

    public bool ChooseExisting => !CreateNew;

    public string NextLabel => Step == 2 ? "Aceptar" : "Siguiente";

    public bool HasChosenFile => ChosenFileName.Length > 0;

    partial void OnChosenFileNameChanged(string value) => OnPropertyChanged(nameof(HasChosenFile));

    partial void OnCreateNewChanged(bool value) => OnPropertyChanged(nameof(ChooseExisting));

    [RelayCommand]
    private async Task OpenAsync()
    {
        if (IsBusy)
            return;

        ResetWizard();
        IsOpen = true;
        IsBusy = true;
        try
        {
            await LoadSectionsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar las comisiones.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Close() => Cancel();

    [RelayCommand]
    private void Cancel()
    {
        if (IsBusy)
            return;

        ResetWizard();
        IsOpen = false;
    }

    [RelayCommand]
    private async Task AcceptAsync()
    {
        if (IsBusy)
            return;

        await ConfirmAsync();
    }

    [RelayCommand]
    private async Task PickFileAsync()
    {
        var path = await _file.PickPathAsync();
        if (path is null)
            return;

        await AcceptFileAsync(path);
    }

    public async Task AcceptFileAsync(string? path)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        BusyText = "Leyendo el archivo…";
        try
        {
            var progress = new Progress<ImportProgress>(value =>
                BusyText = $"Leyendo el archivo… {value.Done:N0} filas");
            var file = await _file.LoadAsync(path, progress);
            _rows = file.Rows;
            ChosenFileName = file.Name;
            ForgetPreview();
            ErrorMessage = string.Empty;
        }
        catch (ImportValidationException ex)
        {
            ClearFile(ex.Message);
        }
        catch (Exception ex)
        {
            ClearFile("No se pudo leer el archivo.");
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
            BusyText = string.Empty;
        }
    }

    [RelayCommand]
    private void Back()
    {
        if (IsBusy)
            return;

        ErrorMessage = string.Empty;
        if (Step == 0)
        {
            Cancel();
            return;
        }

        Step--;
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        if (IsBusy)
            return;

        ErrorMessage = string.Empty;
        switch (Step)
        {
            case 0:
                AdvanceFromFile();
                break;
            case 1:
                await AdvanceFromDestinationAsync();
                break;
            default:
                await ConfirmAsync();
                break;
        }
    }

    private void AdvanceFromFile()
    {
        if (_rows.Count == 0 || ChosenFileName.Length == 0)
        {
            ErrorMessage = ChosenFileName.Length == 0
                ? "Elegí un archivo CSV."
                : "El CSV no tiene alumnos.";
            return;
        }

        Step = 1;
    }

    private async Task AdvanceFromDestinationAsync()
    {
        var error = DestinationError();
        if (error is not null)
        {
            ErrorMessage = error;
            return;
        }

        await LoadPreviewAsync();
    }

    private string? DestinationError()
    {
        if (CreateNew)
            return SectionNameRule.Error(NewName, Sections);

        return SelectedSection is null ? "Elegí una comisión." : null;
    }

    private async Task LoadPreviewAsync()
    {
        IsBusy = true;
        BusyText = "Revisando DNI y legajos…";
        try
        {
            var sectionId = CreateNew ? (int?)null : SelectedSection!.Id;
            var rows = _rows;
            ShowPreview(await Task.Run(() => _import.PreviewAsync(sectionId, rows)));
        }
        catch (ImportValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudo preparar la importación.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
            BusyText = string.Empty;
        }
    }

    private void ShowPreview(ImportPreview preview)
    {
        _preview = preview;
        _choices = preview.Conflicts.Select(conflict => new ConflictChoice(conflict)).ToList();
        Conflicts.Clear();
        foreach (var choice in _choices.Take(MaxConflictCards))
            Conflicts.Add(choice);

        var hidden = _choices.Count - Conflicts.Count;
        MoreConflictsText = hidden > 0
            ? $"Y {hidden:N0} filas más con conflicto, que se omiten."
            : string.Empty;
        InsertText = preview.InsertCount == 1
            ? "Se va a insertar 1 alumno."
            : $"Se van a insertar {preview.InsertCount:N0} alumnos.";
        BlockedText = CreateNew && preview.Conflicts.Count > 0
            ? $"{preview.Conflicts.Count:N0} filas se omiten porque su DNI o legajo ya está guardado o está repetido en el archivo."
            : string.Empty;
        Step = 2;
    }

    // Confirmar cierra el asistente y deja la importación corriendo en segundo plano;
    // el avance y el resultado se ven en el aviso de ImportJobTracker.
    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (IsBusy)
            return;

        if (_preview is null || _rows.Count == 0 || ChosenFileName.Length == 0)
        {
            ErrorMessage = "Elegí un archivo CSV.";
            Step = 0;
            return;
        }

        if (!_jobs.IsIdle)
        {
            ErrorMessage = "Ya hay una importación en curso. Esperá a que termine.";
            return;
        }

        var title = CreateNew ? $"Importando '{NewName.Trim()}'" : $"Importando en '{SelectedSection!.Name}'";
        var job = BuildJob(_preview);
        ResetWizard();
        IsOpen = false;
        var result = await _jobs.RunAsync(title, job);
        if (result is { HasRowErrors: false })
            Imported?.Invoke(result.Message);
    }

    // Toma los datos del asistente ahora: el trabajo corre después, con el asistente ya limpio.
    private Func<IProgress<ImportProgress>, Task<ImportJobResult>> BuildJob(ImportPreview preview)
    {
        var today = _clock.Today;
        if (CreateNew)
        {
            var name = NewName;
            return async progress =>
            {
                var section = await _import.CreateFromPreviewAsync(name, preview, today, progress);
                await _selection.ReloadAsync(section);
                var omitted = preview.Conflicts.Count > 0 ? $" Se omitieron {preview.Conflicts.Count:N0} filas." : string.Empty;
                return new ImportJobResult($"Comisión '{section.Name}' importada correctamente ({preview.InsertCount:N0} alumnos).{omitted}", false);
            };
        }

        var target = SelectedSection!;
        var decisions = _choices.Select(choice => choice.Decision).ToList();
        return async progress =>
        {
            var report = await _import.ApplyWithReportAsync(target.Id, preview, decisions, today, progress);
            await _selection.ReloadAsync(target);
            return report.HasErrors
                ? new ImportJobResult(DescribeErrors(report), true)
                : new ImportJobResult($"Importación en '{target.Name}' completada ({report.Inserted:N0} alumnos).", false);
        };
    }

    // Resumen para el asistente: cuántas entraron y las primeras filas rechazadas con su línea.
    private static string DescribeErrors(ImportResult report)
    {
        const int shown = 5;
        var lines = report.Errors.Take(shown).Select(error => $"línea {error.Line}: {error.Message}");
        var more = report.Errors.Count > shown ? $" (y {report.Errors.Count - shown} más)" : string.Empty;
        return $"Se importaron {report.Inserted:N0} alumnos. {report.Errors.Count:N0} filas con error: {string.Join("; ", lines)}{more}.";
    }

    private void ResetWizard()
    {
        Step = 0;
        ChosenFileName = string.Empty;
        ErrorMessage = string.Empty;
        CreateNew = true;
        NewName = string.Empty;
        InsertText = string.Empty;
        BlockedText = string.Empty;
        _rows = [];
        _preview = null;
        _choices = [];
        Conflicts.Clear();
        MoreConflictsText = string.Empty;
        BusyText = string.Empty;
        Sections.Clear();
    }

    private async Task LoadSectionsAsync()
    {
        var sections = await _import.GetSectionsAsync();
        foreach (var section in sections)
            Sections.Add(section);
        SelectedSection = Sections.FirstOrDefault(section => section.Id == _selection.Current?.Id) ?? Sections.FirstOrDefault();
    }

    private void ClearFile(string message)
    {
        _rows = [];
        ChosenFileName = string.Empty;
        ForgetPreview();
        ErrorMessage = message;
    }

    private void ForgetPreview()
    {
        _preview = null;
        _choices = [];
        Conflicts.Clear();
        MoreConflictsText = string.Empty;
        InsertText = string.Empty;
        BlockedText = string.Empty;
        if (Step > 0)
            Step = 0;
    }
}

public partial class ConflictChoice : ObservableObject
{
    public ConflictChoice(ImportConflict conflict)
    {
        Conflict = conflict;
    }

    public ImportConflict Conflict { get; }

    public string Explanation => Conflict.Explanation;

    public string Title => $"{Conflict.Row.LastName}, {Conflict.Row.FirstName}";

    public string Detail => $"DNI {Conflict.Row.Dni} · legajo {Conflict.Row.StudentNumber}";

    public bool CanMerge => Conflict.CanMerge;

    public bool CannotMerge => !CanMerge;

    [ObservableProperty]
    public partial bool Merge { get; set; }

    public ImportDecision Decision => Merge && CanMerge ? ImportDecision.Merge : ImportDecision.Omit;

    [RelayCommand]
    private void ChooseOmit() => Merge = false;

    [RelayCommand]
    private void ChooseMerge()
    {
        if (CanMerge)
            Merge = true;
    }
}
