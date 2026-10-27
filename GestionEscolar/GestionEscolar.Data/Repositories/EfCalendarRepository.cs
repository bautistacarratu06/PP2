using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfCalendarRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : ICalendarRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<ClassSession>> GetClassSessionsAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.ClassSessions
            .AsNoTracking()
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.SessionDate)
            .Select(s => new { s.Id, s.SectionId, s.SessionDate })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(s => new ClassSession(s.Id, s.SectionId, s.SessionDate)).ToList();
    }

    public async Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Assignments
            .AsNoTracking()
            .Where(a => a.SectionId == sectionId)
            .OrderBy(a => a.DueDate)
            .ThenBy(a => a.Name)
            .Select(a => new { a.Id, a.SectionId, a.Name, a.DueDate })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(a => new Assignment(a.Id, a.SectionId, a.Name, a.DueDate)).ToList();
    }

    public async Task<int> AddClassSessionAsync(int sectionId, DateTime date, CancellationToken cancellationToken = default)
    {
        var day = date.Date;
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var session = new ClassSessionEntity { SectionId = sectionId, SessionDate = day };
        db.ClassSessions.Add(session);
        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return session.Id;
        }
        catch (DbUpdateException ex) when (UniqueConstraint.IsViolation(ex))
        {
            throw new CalendarValidationException($"Ya hay una clase el {day:dd/MM/yyyy} en esta comisión.");
        }
    }

    public async Task<int> AddAssignmentAsync(int sectionId, string name, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var assignment = new AssignmentEntity
        {
            SectionId = sectionId,
            Name = name,
            DueDate = dueDate.Date
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return assignment.Id;
    }
}
