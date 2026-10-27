using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfStudentRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : IStudentRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Students
            .AsNoTracking()
            .Where(s => s.SectionId == sectionId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.StudentNumber, s.Phone, s.Email })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(s => new Student
        {
            Id = s.Id,
            Name = s.FirstName + " " + s.LastName,
            StudentNumber = s.StudentNumber,
            Phone = s.Phone ?? string.Empty,
            Email = s.Email ?? string.Empty
        }).ToList();
    }
}
