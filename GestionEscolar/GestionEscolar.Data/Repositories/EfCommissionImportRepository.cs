using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

public sealed class EfCommissionImportRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : ICommissionImportRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Sections
            .AsNoTracking()
            .OrderBy(section => section.Id)
            .Select(section => new { section.Id, section.Name })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(section => new Section(section.Id, section.Name)).ToList();
    }

    public async Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Students
            .AsNoTracking()
            .Select(student => new
            {
                student.Id,
                student.SectionId,
                student.Dni,
                student.LastName,
                student.FirstName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                student.EnrolledOn
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(student => new Student
        {
            Id = student.Id,
            SectionId = student.SectionId,
            Dni = student.Dni,
            LastName = student.LastName,
            FirstName = student.FirstName,
            Name = student.FirstName + " " + student.LastName,
            StudentNumber = student.StudentNumber,
            Phone = student.Phone ?? string.Empty,
            Email = student.Email ?? string.Empty,
            EnrolledOn = student.EnrolledOn
        }).ToList();
    }

    public async Task<Section> CreateSectionAsync(string name, IReadOnlyList<Student> students, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(students);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var section = new SectionEntity
        {
            Name = name,
            Students = students.Select(student => new StudentEntity
            {
                Dni = student.Dni,
                StudentNumber = student.StudentNumber,
                LastName = student.LastName,
                FirstName = student.FirstName,
                Phone = EmptyToNull(student.Phone),
                Email = EmptyToNull(student.Email),
                EnrolledOn = student.EnrolledOn.Date
            }).ToList()
        };
        db.Sections.Add(section);
        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (UniqueConstraint.IsViolation(ex))
        {
            throw new ImportValidationException("Ya existe una comisión con ese nombre.");
        }

        return new Section(section.Id, section.Name);
    }

    public async Task ApplyAsync(
        int sectionId,
        IReadOnlyList<Student> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inserts);
        ArgumentNullException.ThrowIfNull(updates);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        if (updates.Count > 0)
        {
            var ids = updates.Select(update => update.StudentId).ToList();
            var stored = await db.Students
                .Where(student => ids.Contains(student.Id))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            foreach (var update in updates)
            {
                var student = stored.Single(item => item.Id == update.StudentId);
                student.LastName = update.LastName;
                student.FirstName = update.FirstName;
                student.Phone = EmptyToNull(update.Phone);
                student.Email = EmptyToNull(update.Email);
            }
        }

        foreach (var insert in inserts)
        {
            db.Students.Add(new StudentEntity
            {
                SectionId = sectionId,
                Dni = insert.Dni,
                StudentNumber = insert.StudentNumber,
                LastName = insert.LastName,
                FirstName = insert.FirstName,
                Phone = EmptyToNull(insert.Phone),
                Email = EmptyToNull(insert.Email),
                EnrolledOn = insert.EnrolledOn.Date
            });
        }

        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
