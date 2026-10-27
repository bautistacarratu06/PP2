using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfRiskSettingsRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : IRiskSettingsRepository
{
    public const string HighKey = "high_threshold";
    public const string MediumKey = "medium_threshold";

    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<RiskConfig> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Settings
            .AsNoTracking()
            .Where(setting => setting.Key == HighKey || setting.Key == MediumKey)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var high = rows.SingleOrDefault(setting => setting.Key == HighKey);
        var medium = rows.SingleOrDefault(setting => setting.Key == MediumKey);
        if (high is null || medium is null)
            throw new InvalidOperationException("Faltan los umbrales en dbo.setting.");

        return new RiskConfig { HighThreshold = high.Value, MediumThreshold = medium.Value };
    }

    public async Task SaveAsync(int highThreshold, int mediumThreshold, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        var rows = await db.Settings
            .Where(setting => setting.Key == HighKey || setting.Key == MediumKey)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        if (rows.Count != 2)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("Faltan los umbrales en dbo.setting.");
        }

        rows.Single(setting => setting.Key == HighKey).Value = highThreshold;
        rows.Single(setting => setting.Key == MediumKey).Value = mediumThreshold;
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
