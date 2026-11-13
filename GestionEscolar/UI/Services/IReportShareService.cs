namespace UI.Services;

// Comparte un archivo de texto generado (caché local + diálogo nativo del SO).
public interface IReportShareService
{
    Task ShareTextFileAsync(string fileName, string contents, CancellationToken cancellationToken = default);
}

public sealed class MauiReportShareService : IReportShareService
{
    public async Task ShareTextFileAsync(string fileName, string contents, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(contents);

        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(path, contents, cancellationToken).ConfigureAwait(false);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Exportar reporte",
            File = new ShareFile(path)
        }).ConfigureAwait(false);
    }
}
