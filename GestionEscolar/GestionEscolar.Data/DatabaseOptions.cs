namespace GestionEscolar.Data;

// Configuración de la base. Transporta la connection string leída de la
// configuración de la aplicación hacia la infraestructura.
public sealed class DatabaseOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}
