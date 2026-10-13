using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

// Repositorio en memoria para testear servicios sin base de datos.
public sealed class FakeTrackingRepository : ITrackingRepository
{
    private readonly Dictionary<int, List<StudentTrackingData>> _rowsBySection = new();

    public int? LastRequestedSection { get; private set; }

    public FakeTrackingRepository WithSection(int sectionId, params StudentTrackingData[] rows)
    {
        _rowsBySection[sectionId] = rows.ToList();
        return this;
    }

    public Task<IReadOnlyList<StudentTrackingData>> GetRowsBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        LastRequestedSection = sectionId;
        IReadOnlyList<StudentTrackingData> rows = _rowsBySection.TryGetValue(sectionId, out var list)
            ? list
            : new List<StudentTrackingData>();
        return Task.FromResult(rows);
    }
}
