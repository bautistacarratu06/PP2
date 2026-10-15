using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

public sealed class SqlSectionRepository : ISectionRepository
{
    private const string Sql = "SELECT id, name FROM dbo.section ORDER BY id;";

    private readonly DatabaseInstance _db;

    public SqlSectionRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sections = new List<Section>();

        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(Sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            sections.Add(new Section(reader.GetInt32(0), reader.GetString(1)));

        return sections;
    }
}
