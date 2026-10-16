using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeStudentDetailsRepository : IStudentDetailsRepository
{
    public Dictionary<int, StudentDetail> Students { get; } = new();
    public List<AttendanceRecord> Attendance { get; } = new();
    public List<Submission> Submissions { get; } = new();
    public List<Intervention> Interventions { get; } = new();
    public Dictionary<int, int> ClassSessionSection { get; } = new();

    public int? LastAttendanceSection { get; private set; }
    public int? LastSubmissionsSection { get; private set; }

    public Task<StudentDetail?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Students.TryGetValue(studentId, out var s) ? s : null);

    public Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(int studentId, int sectionId, CancellationToken cancellationToken = default)
    {
        LastAttendanceSection = sectionId;
        IReadOnlyList<AttendanceRecord> rows = Attendance
            .Where(a => a.StudentId == studentId && ClassSessionSection.GetValueOrDefault(a.ClassSessionId) == sectionId)
            .ToList();
        return Task.FromResult(rows);
    }

    public Task<IReadOnlyList<Submission>> GetSubmissionsAsync(int studentId, int sectionId, CancellationToken cancellationToken = default)
    {
        LastSubmissionsSection = sectionId;
        IReadOnlyList<Submission> rows = Submissions.Where(s => s.StudentId == studentId && s.SectionId == sectionId).ToList();
        return Task.FromResult(rows);
    }

    public Task<IReadOnlyList<Intervention>> GetInterventionsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Intervention> rows = Interventions.Where(i => i.StudentId == studentId).ToList();
        return Task.FromResult(rows);
    }
}
