using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

// Guarda en memoria con la misma semántica de upsert que SQL (una fila por clase/trabajo + alumno).
public sealed class FakeAttendanceRepository : IAttendanceRepository
{
    public Dictionary<int, List<(int Id, string Name)>> StudentsBySession { get; } = new();
    public Dictionary<(int Session, int Student), AttendanceStatus> Records { get; } = new();

    public Task<IReadOnlyList<AttendanceMark>> GetMarksAsync(int classSessionId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<AttendanceMark>>(StudentsBySession.GetValueOrDefault(classSessionId, [])
            .Select(s => new AttendanceMark(s.Id, s.Name,
                Records.TryGetValue((classSessionId, s.Id), out var st) ? st : null))
            .ToList());

    public Task SaveAsync(int classSessionId, int studentId, AttendanceStatus status, CancellationToken cancellationToken = default)
    {
        Records[(classSessionId, studentId)] = status;
        return Task.CompletedTask;
    }

    public bool FailNextSaveMany { get; set; }

    public Task SaveManyAsync(
        int classSessionId,
        IReadOnlyList<int> studentIds,
        AttendanceStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentIds);
        if (FailNextSaveMany)
            throw new InvalidOperationException("Fallo simulado al guardar asistencias.");

        foreach (var studentId in studentIds)
            Records[(classSessionId, studentId)] = status;
        return Task.CompletedTask;
    }
}

public sealed class FakeSubmissionRepository : ISubmissionRepository
{
    public Dictionary<int, List<(int Id, string Name)>> StudentsByAssignment { get; } = new();
    public Dictionary<(int Assignment, int Student), SubmissionStatus> Records { get; } = new();

    public Task<IReadOnlyList<SubmissionMark>> GetMarksAsync(int assignmentId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SubmissionMark>>(StudentsByAssignment.GetValueOrDefault(assignmentId, [])
            .Select(s => new SubmissionMark(s.Id, s.Name,
                Records.TryGetValue((assignmentId, s.Id), out var st) ? st : null))
            .ToList());

    public Task SaveAsync(int assignmentId, int studentId, SubmissionStatus status, CancellationToken cancellationToken = default)
    {
        Records[(assignmentId, studentId)] = status;
        return Task.CompletedTask;
    }

    public bool FailNextSaveMany { get; set; }

    public Task SaveManyAsync(
        int assignmentId,
        IReadOnlyList<int> studentIds,
        SubmissionStatus status,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studentIds);
        if (FailNextSaveMany)
            throw new InvalidOperationException("Fallo simulado al guardar entregas.");

        foreach (var studentId in studentIds)
            Records[(assignmentId, studentId)] = status;
        return Task.CompletedTask;
    }
}
