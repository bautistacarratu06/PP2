using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;
using UI.ViewModels;
using Xunit;

namespace GestionEscolar.UI.Tests;

public class SettingsSmokeTests
{
    [Fact]
    public async Task LoadCommand_PopulatesThresholdsFromRepository()
    {
        var repo = new FakeRiskSettingsRepository(highThreshold: 60, mediumThreshold: 30);
        var config = new RiskConfig { HighThreshold = 50, MediumThreshold = 25 };
        var service = new RiskSettingsService(repo, config);
        var vm = new SettingsViewModel(service, config);

        await vm.LoadCommand.ExecuteAsync(null);

        Assert.Equal("60", vm.HighThreshold);
        Assert.Equal("30", vm.MediumThreshold);
        Assert.True(vm.CanSave);
    }
}
