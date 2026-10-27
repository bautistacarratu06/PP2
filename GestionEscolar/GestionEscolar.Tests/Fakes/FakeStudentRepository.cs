using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = [];
    private int _nextId = 1;

    public int? LastRequestedSection { get; private set; }

    public FakeStudentRepository WithSection(int sectionId, params Student[] students)
    {
        _students.RemoveAll(student => student.SectionId == sectionId);
        foreach (var student in students)
        {
            var copy = Clone(student);
            copy.SectionId = sectionId;
            if (copy.Id == 0)
                copy.Id = _nextId++;
            else
                _nextId = Math.Max(_nextId, copy.Id + 1);
            _students.Add(copy);
        }

        return this;
    }

    public Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        LastRequestedSection = sectionId;
        IReadOnlyList<Student> rows = _students
            .Where(student => student.SectionId == sectionId)
            .OrderBy(student => student.LastName, StringComparer.CurrentCulture)
            .ThenBy(student => student.FirstName, StringComparer.CurrentCulture)
            .Select(Clone)
            .ToList();
        return Task.FromResult(rows);
    }

    public Task<Student?> FindByDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var found = _students.FirstOrDefault(student => student.Dni == dni);
        return Task.FromResult(found is null ? null : Clone(found));
    }

    public Task<Student?> FindByStudentNumberAsync(int sectionId, string studentNumber, CancellationToken cancellationToken = default)
    {
        var found = _students.FirstOrDefault(student => student.SectionId == sectionId && student.StudentNumber == studentNumber);
        return Task.FromResult(found is null ? null : Clone(found));
    }

    public Task<Student> AddAsync(Student student, CancellationToken cancellationToken = default)
    {
        var stored = Clone(student);
        stored.Id = _nextId++;
        _students.Add(stored);
        return Task.FromResult(Clone(stored));
    }

    private static Student Clone(Student student) => new()
    {
        Id = student.Id,
        SectionId = student.SectionId,
        Dni = student.Dni,
        LastName = student.LastName,
        FirstName = student.FirstName,
        Name = student.Name,
        StudentNumber = student.StudentNumber,
        Phone = student.Phone,
        Email = student.Email,
        EnrolledOn = student.EnrolledOn
    };
}
