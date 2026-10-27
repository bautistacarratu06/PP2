using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Tests.Fakes;

public sealed class FakeCommissionImportRepository : ICommissionImportRepository
{
    private readonly List<Section> _sections = [];
    private readonly List<Student> _students = [];
    private int _nextSectionId = 1;
    private int _nextStudentId = 1;

    public IReadOnlyList<Section> Sections => _sections;

    public IReadOnlyList<Student> Students => _students;

    public void Seed(Section section, params Student[] students)
    {
        _sections.Add(section);
        _nextSectionId = Math.Max(_nextSectionId, section.Id + 1);
        foreach (var student in students)
        {
            var copy = Copy(student);
            copy.SectionId = section.Id;
            if (copy.Id == 0)
                copy.Id = _nextStudentId++;
            else
                _nextStudentId = Math.Max(_nextStudentId, copy.Id + 1);
            _students.Add(copy);
        }
    }

    public Task<IReadOnlyList<Section>> GetSectionsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Section>>(_sections.ToList());

    public Task<IReadOnlyList<Student>> GetStudentsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Student>>(_students.Select(Copy).ToList());

    public Task<Section> CreateSectionAsync(string name, IReadOnlyList<Student> students, CancellationToken cancellationToken = default)
    {
        var section = new Section(_nextSectionId++, name);
        _sections.Add(section);
        foreach (var student in students)
        {
            var copy = Copy(student);
            copy.Id = _nextStudentId++;
            copy.SectionId = section.Id;
            _students.Add(copy);
        }

        return Task.FromResult(section);
    }

    public Task ApplyAsync(
        int sectionId,
        IReadOnlyList<Student> inserts,
        IReadOnlyList<StudentContactChange> updates,
        CancellationToken cancellationToken = default)
    {
        foreach (var update in updates)
        {
            var student = _students.Single(item => item.Id == update.StudentId);
            student.LastName = update.LastName;
            student.FirstName = update.FirstName;
            student.Name = $"{update.FirstName} {update.LastName}";
            student.Phone = update.Phone;
            student.Email = update.Email;
        }

        foreach (var insert in inserts)
        {
            var copy = Copy(insert);
            copy.Id = _nextStudentId++;
            copy.SectionId = sectionId;
            _students.Add(copy);
        }

        return Task.CompletedTask;
    }

    private static Student Copy(Student student) => new()
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
