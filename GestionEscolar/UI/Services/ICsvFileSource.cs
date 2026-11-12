namespace UI.Services;

// Elegir un CSV y leer su texto, sin que el ViewModel toque el selector de archivos ni el disco.
public interface ICsvFileSource
{
    // null si la persona cancela.
    Task<string?> PickPathAsync();

    Task<string> ReadAllTextAsync(string path);
}

public sealed class DeviceCsvFileSource : ICsvFileSource
{
    public async Task<string?> PickPathAsync()
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Seleccionar CSV" });
        return file?.FullPath;
    }

    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);
}
