using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeSectionRepository(params Section[] sections) : ISectionRepository
{
    public Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Section>>(sections);
}
