using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeRiskSettingsRepository : IRiskSettingsRepository
{
    public FakeRiskSettingsRepository(int highThreshold = 50, int mediumThreshold = 25)
    {
        HighThreshold = highThreshold;
        MediumThreshold = mediumThreshold;
    }

    public int HighThreshold { get; private set; }

    public int MediumThreshold { get; private set; }

    public int SaveCount { get; private set; }

    public Task<RiskConfig> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new RiskConfig
        {
            HighThreshold = HighThreshold,
            MediumThreshold = MediumThreshold
        });

    public Task SaveAsync(int highThreshold, int mediumThreshold, CancellationToken cancellationToken = default)
    {
        HighThreshold = highThreshold;
        MediumThreshold = mediumThreshold;
        SaveCount++;
        return Task.CompletedTask;
    }
}
