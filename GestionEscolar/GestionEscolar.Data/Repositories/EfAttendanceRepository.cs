using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfAttendanceRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : IAttendanceRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<AttendanceMark>> GetMarksAsync(int classSessionId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await (
            from session in db.ClassSessions.AsNoTracking()
            where session.Id == classSessionId
            from student in session.Section.Students
            orderby student.LastName, student.FirstName
            select new
            {
                student.Id,
                student.LastName,
                student.FirstName,
                Status = student.AttendanceRecords
                    .Where(record => record.ClassSessionId == session.Id)
                    .Select(record => record.Status)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(row => new AttendanceMark(
                row.Id,
                row.LastName + ", " + row.FirstName,
                DbStatusValues.ParseAttendanceStatus(row.Status)))
            .ToList();
    }

    public async Task SaveAsync(int classSessionId, int studentId, AttendanceStatus status, CancellationToken cancellationToken = default)
    {
        var stored = status.ToDbValue();
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        var existing = await db.AttendanceRecords
            .FirstOrDefaultAsync(
                record => record.ClassSessionId == classSessionId && record.StudentId == studentId,
                cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            if (existing.Status == stored)
                return;

            existing.Status = stored;
            existing.CorrectedAt = Clock.Now();
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        var enrolled = await db.ClassSessions
            .AnyAsync(
                session => session.Id == classSessionId
                    && session.Section.Students.Any(student => student.Id == studentId),
                cancellationToken)
            .ConfigureAwait(false);
        if (!enrolled)
            return;

        db.AttendanceRecords.Add(new AttendanceRecordEntity
        {
            ClassSessionId = classSessionId,
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
            var winner = await db.AttendanceRecords
                .FirstAsync(
                    record => record.ClassSessionId == classSessionId && record.StudentId == studentId,
                    cancellationToken)
                .ConfigureAwait(false);
            if (winner.Status == stored)
                return;

            winner.Status = stored;
            winner.CorrectedAt = Clock.Now();
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private static class Clock
    {
        public static DateTime Now()
        {
            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }
    }
}
