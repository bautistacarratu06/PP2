using Microsoft.Data.SqlClient; // NuGet Microsoft.Data.SqlClient

namespace GestionEscolar.Data;

// Fábrica de conexiones a SQL Server.
// Antes era un singleton manual (GetInstance); ahora es singleton por inyección de
// dependencias (AddSingleton en MauiProgram) y recibe la connection string por configuración.
public sealed class DatabaseInstance
{
    private readonly string _connectionString;

    public DatabaseInstance(DatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException("Falta la connection string.", nameof(options));

        _connectionString = options.ConnectionString;
    }

    // Devuelve una conexión nueva (sin abrir) para ejecutar consultas
    public SqlConnection GetConnection() => new(_connectionString);

    // Abre y devuelve una conexión lista para usar
    public async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var conn = GetConnection();
        try
        {
            await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
            return conn;
        }
        catch
        {
            await conn.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    // Devuelve true si se puede abrir una conexión contra la base
    public bool TestConnection()
    {
        try
        {
            using SqlConnection conn = GetConnection();
            conn.Open();
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var conn = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (SqlException)
        {
            return false;
        }
    }

    private const string ListTablesSql =
        @"SELECT TABLE_SCHEMA, TABLE_NAME
          FROM INFORMATION_SCHEMA.TABLES
          WHERE TABLE_TYPE = 'BASE TABLE'
          ORDER BY TABLE_SCHEMA, TABLE_NAME";

    // Lista las tablas de usuario como "schema.tabla"
    public List<string> ListTables()
    {
        var tables = new List<string>();

        using SqlConnection conn = GetConnection();
        conn.Open();
        using var cmd = new SqlCommand(ListTablesSql, conn);
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
            tables.Add($"{reader.GetString(0)}.{reader.GetString(1)}");

        return tables;
    }

    public async Task<List<string>> ListTablesAsync(CancellationToken cancellationToken = default)
    {
        var tables = new List<string>();

        await using var conn = await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(ListTablesSql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            tables.Add($"{reader.GetString(0)}.{reader.GetString(1)}");

        return tables;
    }
}
