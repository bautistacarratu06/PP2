using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.Services;

// Una sola comisión elegida para toda la ventana. El selector del marco y las
// pantallas que ya recargan al cambiar de comisión leen y escriben acá.
public sealed class SectionSelection
{
    private readonly SectionService _sections;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public SectionSelection(SectionService sections)
    {
        _sections = sections;
    }

    public IReadOnlyList<Section> Items { get; private set; } = [];

    public Section? Current { get; private set; }

    public event EventHandler? Changed;

    public async Task EnsureLoadedAsync()
    {
        if (Items.Count > 0)
            return;

        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (Items.Count > 0)
                return;

            var items = await _sections.GetAllAsync().ConfigureAwait(false);
            Items = items;
            Current ??= SectionService.PickDefault(items);
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Select(Section? section)
    {
        if (section is null || Current?.Id == section.Id)
            return;

        Current = section;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Vuelve a leer las comisiones y deja elegida la que se acaba de crear o importar.
    public async Task ReloadAsync(Section section)
    {
        ArgumentNullException.ThrowIfNull(section);
        var items = await _sections.GetAllAsync().ConfigureAwait(false);
        Items = items;
        Current = items.FirstOrDefault(item => item.Id == section.Id) ?? section;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
