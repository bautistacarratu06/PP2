using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfSubmissionRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : ISubmissionRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<SubmissionMark>> GetMarksAsync(int assignmentId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await (
            from assignment in db.Assignments.AsNoTracking()
            where assignment.Id == assignmentId
            from student in assignment.Section.Students
            orderby student.LastName, student.FirstName
            select new
            {
                student.Id,
                student.LastName,
                student.FirstName,
                Status = student.Submissions
                    .Where(submission => submission.AssignmentId == assignment.Id)
                    .Select(submission => submission.Status)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(row => new SubmissionMark(
                row.Id,
                row.LastName + ", " + row.FirstName,
                DbStatusValues.ParseSubmissionStatus(row.Status)))
            .ToList();
    }

    public async Task SaveAsync(int assignmentId, int studentId, SubmissionStatus status, CancellationToken cancellationToken = default)
    {
        var stored = status.ToDbValue();
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var existing = await db.Submissions
            .FirstOrDefaultAsync(
                submission => submission.AssignmentId == assignmentId && submission.StudentId == studentId,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            existing.Status = stored;
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var enrolled = await db.Assignments
            .AnyAsync(
                assignment => assignment.Id == assignmentId
                    && assignment.Section.Students.Any(student => student.Id == studentId),
                cancellationToken)
            .ConfigureAwait(false);
        if (!enrolled)
            return;

        db.Submissions.Add(new SubmissionEntity
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            Status = stored
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (UniqueConstraint.IsViolation(ex))
        {
            db.ChangeTracker.Clear();
            var winner = await db.Submissions
                .FirstAsync(
                    submission => submission.AssignmentId == assignmentId && submission.StudentId == studentId,
                    cancellationToken)
                .ConfigureAwait(false);
            winner.Status = stored;
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
