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
    private IReadOnlyList<RosterCsvRow> _rows = [];
    private ImportPreview? _preview;

    public CommissionImportViewModel(
        ICommissionImportService import,
        SectionSelection selection,
        CommissionImportFileStep file,
        IClock clock)
    {
        _import = import;
        _selection = selection;
        _file = file;
        _clock = clock;
    }

    public ObservableCollection<Section> Sections { get; } = new();

    public ObservableCollection<ConflictChoice> Conflicts { get; } = new();

    [ObservableProperty]
    public partial bool IsOpen { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFileStep), nameof(IsDestinationStep), nameof(IsPreviewStep), nameof(NextLabel))]
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
    public partial string BlockedText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsFileStep => Step == 0;

    public bool IsDestinationStep => Step == 1;

    public bool IsPreviewStep => Step == 2;

    public bool ChooseExisting => !CreateNew;

    public string NextLabel => Step == 2 ? "Confirmar" : "Siguiente";

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
    private void Close() => IsOpen = false;

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
        try
        {
            var file = await _file.LoadAsync(path);
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
        }
    }

    [RelayCommand]
    private void Back()
    {
        ErrorMessage = string.Empty;
        if (Step == 0)
        {
            IsOpen = false;
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
        try
        {
            var sectionId = CreateNew ? (int?)null : SelectedSection!.Id;
            ShowPreview(await _import.PreviewAsync(sectionId, _rows));
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
        }
    }

    private void ShowPreview(ImportPreview preview)
    {
        _preview = preview;
        Conflicts.Clear();
        foreach (var conflict in preview.Conflicts)
            Conflicts.Add(new ConflictChoice(conflict));

        InsertText = preview.InsertCount == 1
            ? "Se va a insertar 1 alumno."
            : $"Se van a insertar {preview.InsertCount} alumnos.";
        BlockedText = CreateNew && preview.Conflicts.Count > 0
            ? $"{preview.Conflicts.Count} filas se omiten porque su DNI o legajo ya está guardado o está repetido en el archivo."
            : string.Empty;
        Step = 2;
    }

    private async Task ConfirmAsync()
    {
        if (_preview is null || _rows.Count == 0 || ChosenFileName.Length == 0)
        {
            ErrorMessage = "Elegí un archivo CSV.";
            Step = 0;
            return;
        }

        IsBusy = true;
        try
        {
            var (section, report) = await RunImportAsync(_preview);
            await _selection.ReloadAsync(section);
            if (report is { HasErrors: true })
                ErrorMessage = DescribeErrors(report);
            else
                IsOpen = false;
        }
        catch (ImportValidationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudo confirmar la importación.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task<Section> CreateSectionAsync() => _import.CreateAsync(NewName, _rows, _clock.Today);

    private async Task<(Section Section, ImportResult? Report)> RunImportAsync(ImportPreview preview)
    {
        if (CreateNew)
            return (await CreateSectionAsync(), null);

        return await ApplyToExistingAsync(preview);
    }

    private async Task<(Section Section, ImportResult? Report)> ApplyToExistingAsync(ImportPreview preview)
    {
        var section = SelectedSection!;
        var decisions = Conflicts.Select(choice => choice.Decision).ToList();
        var report = await _import.ApplyWithReportAsync(section.Id, preview, decisions, _clock.Today);
        return (section, report);
    }

    // Resumen para el asistente: cuántas entraron y las primeras filas rechazadas con su línea.
    private static string DescribeErrors(ImportResult report)
    {
        const int shown = 5;
        var lines = report.Errors.Take(shown).Select(error => $"línea {error.Line}: {error.Message}");
        var more = report.Errors.Count > shown ? $" (y {report.Errors.Count - shown} más)" : string.Empty;
        return $"Se importaron {report.Inserted} alumnos. {report.Errors.Count} filas con error: {string.Join("; ", lines)}{more}.";
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
        Conflicts.Clear();
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
        Conflicts.Clear();
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
