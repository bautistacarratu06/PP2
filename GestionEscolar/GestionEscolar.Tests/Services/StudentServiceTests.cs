using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class StudentServiceTests
{
    [Fact]
    public async Task Given_SectionWithStudents_When_RequestingRoster_Then_RepositoryReceivesIdAndFieldsAreKept()
    {
        var ana = new Student
        {
            Id = 7,
            Name = "Acosta, Ana",
            StudentNumber = "100",
            Phone = "555",
            Email = "ana@edu"
        };
        var repo = new FakeStudentRepository().WithSection(1, ana);

        var roster = await new StudentService(repo).GetRosterAsync(1);

        Assert.Equal(1, repo.LastRequestedSection);
        var student = Assert.Single(roster);
        Assert.Equal(ana.Id, student.Id);
        Assert.Equal(ana.Name, student.Name);
        Assert.Equal(ana.StudentNumber, student.StudentNumber);
        Assert.Equal(ana.Phone, student.Phone);
        Assert.Equal(ana.Email, student.Email);
    }

    [Fact]
    public async Task Given_EmptySection_When_RequestingRoster_Then_ListIsEmpty()
    {
        var repo = new FakeStudentRepository();

        var roster = await new StudentService(repo).GetRosterAsync(4);

        Assert.Equal(4, repo.LastRequestedSection);
        Assert.Empty(roster);
    }
}
