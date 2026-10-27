using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfSectionRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : ISectionRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Sections
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(s => new Section(s.Id, s.Name)).ToList();
    }
}
