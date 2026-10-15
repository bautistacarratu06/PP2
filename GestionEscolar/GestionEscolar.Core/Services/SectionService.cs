using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Comisiones disponibles y cuál se elige por defecto en Seguimiento.
public class SectionService
{
    public const int PreferredDefaultId = 1;

    private readonly ISectionRepository _repository;

    public SectionService(ISectionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<IReadOnlyList<Section>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    // Por defecto: la comisión 1 si existe; si no, la primera; null si no hay ninguna.
    public static Section? PickDefault(IEnumerable<Section> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);
        var list = sections.ToList();
        return list.FirstOrDefault(s => s.Id == PreferredDefaultId) ?? list.FirstOrDefault();
    }
}
