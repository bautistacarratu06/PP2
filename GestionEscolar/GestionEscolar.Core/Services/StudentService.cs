
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Nómina de una comisión: la lee y da de alta un alumno.
public class StudentService
{
    // Crea una propiedad interna de la clase que es de tipo IStudentRepository y se llama _repository.
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IReadOnlyList<Student>> GetRosterAsync(
        int sectionId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _repository
            .GetRosterAsync(sectionId, cancellationToken)
            .ConfigureAwait(false);

        return rows
            .Select(Copy)
            .OrderBy(student => student.LastName, StringComparer.CurrentCulture)
            .ThenBy(student => student.FirstName, StringComparer.CurrentCulture)
            .ToList();
    }

    public async Task<Student> AddAsync(int sectionId, StudentDraft draft, DateTime today, CancellationToken cancellationToken = default)
    {
        var student = StudentEntryRules.Normalize(draft, today);
        var byNumber = await _repository
            .FindByStudentNumberAsync(sectionId, student.StudentNumber, cancellationToken)
            .ConfigureAwait(false);
        if (byNumber is not null)
            throw new StudentEnrollmentException($"El legajo {student.StudentNumber} ya pertenece a {StudentEntryRules.Label(byNumber)}.");

        var byDni = await _repository.FindByDniAsync(student.Dni, cancellationToken).ConfigureAwait(false);
        if (byDni is not null)
            throw new StudentEnrollmentException($"El DNI {student.Dni} ya pertenece a {StudentEntryRules.Label(byDni)}.");

        student.SectionId = sectionId;
        return Copy(await _repository.AddAsync(student, cancellationToken).ConfigureAwait(false));
    }

    private static Student Copy(Student row) => new()
    {
        Id = row.Id,
        SectionId = row.SectionId,
        Dni = row.Dni,
        LastName = row.LastName,
        FirstName = row.FirstName,
        Name = row.Name,
        StudentNumber = row.StudentNumber,
        Phone = row.Phone,
        Email = row.Email,
        EnrolledOn = row.EnrolledOn
    };
}
