using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeStudentRepository : IStudentRepository
{
    private readonly Dictionary<int, List<Student>> _bySection = new();

    public int? LastRequestedSection { get; private set; }

    public FakeStudentRepository WithSection(int sectionId, params Student[] students)
    {
        _bySection[sectionId] = students.ToList();
        return this;
    }

    public Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        LastRequestedSection = sectionId;
        IReadOnlyList<Student> rows = _bySection.TryGetValue(sectionId, out var list)
            ? list
            : new List<Student>();
        return Task.FromResult(rows);
    }
}
