using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfStudentDetailsRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : IStudentDetailsRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<StudentDetail?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var row = await db.Students
            .AsNoTracking()
            .Where(student => student.Id == studentId)
            .Select(student => new
            {
                student.Id,
                student.FirstName,
                student.LastName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                student.SectionId,
                SectionName = student.Section.Name
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (row is null)
            return null;

        return new StudentDetail
        {
            StudentId = row.Id,
            Name = row.FirstName,
            LastName = row.LastName,
            StudentNumber = row.StudentNumber,
            PhoneNumber = row.Phone ?? string.Empty,
            Email = row.Email ?? string.Empty,
            SectionId = row.SectionId,
            SectionName = row.SectionName
        };
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(
        int studentId,
        int sectionId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.ClassSessions
            .AsNoTracking()
            .Where(session => session.SectionId == sectionId)
            .OrderByDescending(session => session.SessionDate)
            .Select(session => new
            {
                session.Id,
                session.SessionDate,
                RecordId = session.AttendanceRecords
                    .Where(record => record.StudentId == studentId)
                    .Select(record => (int?)record.Id)
                    .FirstOrDefault(),
                Status = session.AttendanceRecords
                    .Where(record => record.StudentId == studentId)
                    .Select(record => record.Status)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(row => new AttendanceRecord
        {
            Id = row.RecordId,
            StudentId = studentId,
            ClassSessionId = row.Id,
            Date = row.SessionDate,
            Status = DbStatusValues.ParseAttendanceStatus(row.Status)
        }).ToList();
    }

    public async Task<IReadOnlyList<Submission>> GetSubmissionsAsync(
        int studentId,
        int sectionId,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Assignments
            .AsNoTracking()
            .Where(assignment => assignment.SectionId == sectionId)
            .OrderBy(assignment => assignment.DueDate)
            .ThenBy(assignment => assignment.Id)
            .Select(assignment => new
            {
                assignment.Id,
                assignment.Name,
                assignment.DueDate,
                SubmissionId = assignment.Submissions
                    .Where(submission => submission.StudentId == studentId)
                    .Select(submission => (int?)submission.Id)
                    .FirstOrDefault(),
                Status = assignment.Submissions
                    .Where(submission => submission.StudentId == studentId)
                    .Select(submission => submission.Status)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(row => new Submission
        {
            Id = row.SubmissionId,
            StudentId = studentId,
            SectionId = sectionId,
            AssignmentId = row.Id,
            AssignmentName = row.Name,
            DueDate = row.DueDate,
            Status = DbStatusValues.ParseSubmissionStatus(row.Status)
        }).ToList();
    }

    public async Task<IReadOnlyList<Intervention>> GetInterventionsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Interventions
            .AsNoTracking()
            .Where(intervention => intervention.StudentId == studentId)
            .OrderByDescending(intervention => intervention.InterventionDate)
            .ThenByDescending(intervention => intervention.Id)
            .Select(intervention => new
            {
                intervention.Id,
                intervention.InterventionDate,
                intervention.Channel,
                intervention.Outcome,
                intervention.Note
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(row => new Intervention
        {
            Id = row.Id,
            StudentId = studentId,
            Date = row.InterventionDate,
            Channel = row.Channel,
            Outcome = row.Outcome,
            Note = row.Note
        }).ToList();
    }

    public async Task<Intervention> AddInterventionAsync(Intervention intervention, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intervention);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var entity = new InterventionEntity
        {
            StudentId = intervention.StudentId,
            InterventionDate = intervention.Date.Date,
            Channel = intervention.Channel,
            Outcome = intervention.Outcome,
            Note = string.IsNullOrWhiteSpace(intervention.Note) ? null : intervention.Note
        };
        db.Interventions.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        intervention.Id = entity.Id;
        intervention.Date = entity.InterventionDate;
        intervention.Note = entity.Note;
        return intervention;
    }
}
