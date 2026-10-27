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
    private readonly CommissionImportService _import;
    private readonly SectionSelection _selection;
    private IReadOnlyList<RosterCsvRow> _rows = [];
    private ImportPreview? _preview;

    public CommissionImportViewModel(CommissionImportService import, SectionSelection selection)
    {
        _import = import;
        _selection = selection;
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
        IsOpen = true;
        try
        {
            var sections = await _import.GetSectionsAsync();
            foreach (var section in sections)
                Sections.Add(section);
            SelectedSection = Sections.FirstOrDefault(section => section.Id == _selection.Current?.Id) ?? Sections.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ErrorMessage = "No se pudieron cargar las comisiones.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    [RelayCommand]
    private void Close() => IsOpen = false;

    [RelayCommand]
    private async Task PickFileAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Seleccionar CSV" });
        if (file is null)
            return;

        await AcceptFileAsync(file.FullPath);
    }

    public async Task AcceptFileAsync(string? path)
    {
        if (!CommissionImportService.IsCsvFile(path))
        {
            ClearFile("El archivo tiene que ser un CSV.");
            return;
        }

        try
        {
            var text = await File.ReadAllTextAsync(path!);
            _rows = _import.Read(new StringReader(text));
            ChosenFileName = Path.GetFileName(path!);
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
        if (Step == 0)
        {
            if (_rows.Count == 0 || ChosenFileName.Length == 0)
            {
                ErrorMessage = ChosenFileName.Length == 0
                    ? "Elegí un archivo CSV."
                    : "El CSV no tiene alumnos.";
                return;
            }

            Step = 1;
            return;
        }

        if (Step == 1)
        {
            if (CreateNew)
            {
                var nameError = StudentEntryRules.SectionNameError(NewName, Sections);
                if (nameError is not null)
                {
                    ErrorMessage = nameError;
                    return;
                }
            }
            else if (SelectedSection is null)
            {
                ErrorMessage = "Elegí una comisión.";
                return;
            }

            await LoadPreviewAsync();
            return;
        }

        await ConfirmAsync();
    }

    private async Task LoadPreviewAsync()
    {
        IsBusy = true;
        try
        {
            var sectionId = CreateNew ? (int?)null : SelectedSection!.Id;
            _preview = await _import.PreviewAsync(sectionId, _rows);
            Conflicts.Clear();
            foreach (var conflict in _preview.Conflicts)
                Conflicts.Add(new ConflictChoice(conflict));

            InsertText = _preview.InsertCount == 1
                ? "Se va a insertar 1 alumno."
                : $"Se van a insertar {_preview.InsertCount} alumnos.";
            BlockedText = CreateNew && _preview.Conflicts.Count > 0
                ? "El archivo tiene DNI o legajos repetidos, o un DNI que ya está guardado. Esta comisión nueva no se confirma."
                : string.Empty;
            Step = 2;
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

    private async Task ConfirmAsync()
    {
        if (_preview is null)
            return;

        if (CreateNew && _preview.Conflicts.Count > 0)
        {
            ErrorMessage = BlockedText;
            return;
        }

        IsBusy = true;
        try
        {
            Section section;
            if (CreateNew)
            {
                section = await _import.CreateAsync(NewName, _rows, DateTime.Today);
            }
            else
            {
                await _import.ApplyAsync(
                    SelectedSection!.Id,
                    _preview,
                    Conflicts.Select(choice => choice.Decision).ToList(),
                    DateTime.Today);
                section = SelectedSection;
            }

            IsOpen = false;
            await _selection.ReloadAsync(section);
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

    private void ClearFile(string message)
    {
        _rows = [];
        ChosenFileName = string.Empty;
        ErrorMessage = message;
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
