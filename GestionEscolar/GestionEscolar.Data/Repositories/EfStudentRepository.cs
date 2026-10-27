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
            .Where(student => student.SectionId == sectionId)
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .Select(student => new StudentRow(
                student.Id,
                student.SectionId,
                student.Dni,
                student.LastName,
                student.FirstName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                student.EnrolledOn))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(Map).ToList();
    }

    public async Task<Student?> FindByDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var row = await db.Students
            .AsNoTracking()
            .Where(student => student.Dni == dni)
            .Select(student => new StudentRow(
                student.Id,
                student.SectionId,
                student.Dni,
                student.LastName,
                student.FirstName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                student.EnrolledOn))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : Map(row);
    }

    public async Task<Student?> FindByStudentNumberAsync(int sectionId, string studentNumber, CancellationToken cancellationToken = default)
    {
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var row = await db.Students
            .AsNoTracking()
            .Where(student => student.SectionId == sectionId && student.StudentNumber == studentNumber)
            .Select(student => new StudentRow(
                student.Id,
                student.SectionId,
                student.Dni,
                student.LastName,
                student.FirstName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                student.EnrolledOn))
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return row is null ? null : Map(row);
    }

    public async Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(student);
        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var entity = new StudentEntity
        {
            SectionId = student.SectionId,
            Dni = student.Dni,
            StudentNumber = student.StudentNumber,
            LastName = student.LastName,
            FirstName = student.FirstName,
            Phone = EmptyToNull(student.Phone),
            Email = EmptyToNull(student.Email),
            EnrolledOn = student.EnrolledOn.Date
        };
        db.Students.Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateException ex) when (UniqueConstraint.IsViolation(ex))
        {
            throw new StudentEnrollmentException("No se pudo guardar porque el DNI o el legajo ya existen.");
        }

        return Map(new StudentRow(
            entity.Id,
            entity.SectionId,
            entity.Dni,
            entity.LastName,
            entity.FirstName,
            entity.StudentNumber,
            entity.Phone,
            entity.Email,
            entity.EnrolledOn));
    }

    private static Student Map(StudentRow row) => new()
    {
        Id = row.Id,
        SectionId = row.SectionId,
        Dni = row.Dni,
        LastName = row.LastName,
        FirstName = row.FirstName,
        Name = row.FirstName + " " + row.LastName,
        StudentNumber = row.StudentNumber,
        Phone = row.Phone ?? string.Empty,
        Email = row.Email ?? string.Empty,
        EnrolledOn = row.EnrolledOn
    };

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record StudentRow(
        int Id,
        int SectionId,
        string Dni,
        string LastName,
        string FirstName,
        string StudentNumber,
        string? Phone,
        string? Email,
        DateTime EnrolledOn);
}
