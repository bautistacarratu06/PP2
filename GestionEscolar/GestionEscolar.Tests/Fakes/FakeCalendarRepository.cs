using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeCalendarRepository : ICalendarRepository
{
    private int _nextId = 100;

    public List<ClassSession> Sessions { get; } = new();
    public List<Assignment> Assignments { get; } = new();

    public Task<IReadOnlyList<ClassSession>> GetClassSessionsAsync(int sectionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ClassSession>>(Sessions.Where(s => s.SectionId == sectionId).ToList());

    public Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(int sectionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Assignment>>(Assignments.Where(a => a.SectionId == sectionId).ToList());

    public Task<int> AddClassSessionAsync(int sectionId, DateTime date, CancellationToken cancellationToken = default)
    {
        var id = _nextId++;
        Sessions.Add(new ClassSession(id, sectionId, date));
        return Task.FromResult(id);
    }

    public Task<int> AddAssignmentAsync(int sectionId, string name, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        var id = _nextId++;
        Assignments.Add(new Assignment(id, sectionId, name, dueDate));
        return Task.FromResult(id);
    }
}
