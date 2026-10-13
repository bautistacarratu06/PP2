namespace GestionEscolar.Data;

// Configuración de la base. Se arma en un único lugar (MauiProgram) y se inyecta.
public sealed class DatabaseOptions
{
    public string ConnectionString { get; init; } = string.Empty;
}
