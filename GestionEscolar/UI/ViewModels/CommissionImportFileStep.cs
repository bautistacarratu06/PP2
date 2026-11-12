using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels;

// Paso "archivo" de la importación: elegir el CSV, leerlo del disco y convertirlo en filas.
// El asistente solo guarda el resultado; no toca el selector de archivos ni el disco.
public sealed class CommissionImportFileStep
{
    private readonly ICsvFileSource _files;
    private readonly ICommissionImportService _import;

    public CommissionImportFileStep(ICsvFileSource files, ICommissionImportService import)
    {
        _files = files;
        _import = import;
    }

    // null si la persona cancela el selector.
    public Task<string?> PickPathAsync() => _files.PickPathAsync();

    // Lanza ImportValidationException si el archivo no es un CSV válido.
    public async Task<ImportFile> LoadAsync(string? path)
    {
        if (!RosterCsvReader.IsCsvFile(path))
            throw new ImportValidationException("El archivo tiene que ser un CSV.");

        var text = await _files.ReadAllTextAsync(path!);
        var rows = _import.Read(new StringReader(text));
        return new ImportFile(rows, Path.GetFileName(path!));
    }
}

public sealed record ImportFile(IReadOnlyList<RosterCsvRow> Rows, string Name);
