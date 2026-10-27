using System.Data;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

// high_threshold y medium_threshold en dbo.setting (sembrados en la migración 0009).
public sealed class SqlRiskSettingsRepository : IRiskSettingsRepository
{
    private const string HighKey = "high_threshold";
    private const string MediumKey = "medium_threshold";

    private const string SelectSql = @"
SELECT setting_key, setting_value
FROM dbo.setting
WHERE setting_key IN (@highKey, @mediumKey);";

    private const string UpdateSql = @"
UPDATE dbo.setting
   SET setting_value = CASE setting_key
                           WHEN @highKey THEN @highValue
                           WHEN @mediumKey THEN @mediumValue
                       END
 WHERE setting_key IN (@highKey, @mediumKey);";

    private const string CountSql = @"
SELECT COUNT(*)
FROM dbo.setting
WHERE setting_key IN (@highKey, @mediumKey);";

    private readonly DatabaseInstance _db;

    public SqlRiskSettingsRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<RiskConfig> GetAsync(CancellationToken cancellationToken = default)
    {
        int? high = null;
        int? medium = null;

        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(SelectSql, conn);
        AddKeys(cmd);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var key = reader.GetString(0);
            var value = reader.GetInt32(1);
            if (key == HighKey)
                high = value;
            else if (key == MediumKey)
                medium = value;
        }

        if (high is null || medium is null)
            throw new InvalidOperationException("Faltan los umbrales en dbo.setting.");

        return new RiskConfig { HighThreshold = high.Value, MediumThreshold = medium.Value };
    }

    public async Task SaveAsync(int highThreshold, int mediumThreshold, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = (SqlTransaction)await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        await using (var update = new SqlCommand(UpdateSql, conn, transaction))
        {
            AddKeys(update);
            update.Parameters.Add("@highValue", SqlDbType.Int).Value = highThreshold;
            update.Parameters.Add("@mediumValue", SqlDbType.Int).Value = mediumThreshold;
            await update.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var count = new SqlCommand(CountSql, conn, transaction);
        AddKeys(count);
        var storedRows = Convert.ToInt32(
            await count.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (storedRows != 2)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("Faltan los umbrales en dbo.setting.");
        }

        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void AddKeys(SqlCommand cmd)
    {
        cmd.Parameters.Add("@highKey", SqlDbType.NVarChar, 40).Value = HighKey;
        cmd.Parameters.Add("@mediumKey", SqlDbType.NVarChar, 40).Value = MediumKey;
    }
}
