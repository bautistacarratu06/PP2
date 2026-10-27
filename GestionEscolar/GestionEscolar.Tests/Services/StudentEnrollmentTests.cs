using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class StudentEnrollmentTests
{
    private static readonly DateTime Today = new(2026, 10, 27);

    [Fact]
    public async Task Given_NewStudent_When_Adding_Then_OnlyThatSectionListsThemAndKeepsTheEnrollmentDate()
    {
        var repo = new FakeStudentRepository()
            .WithSection(1, Existing("10", "100", "Alba", "Bea"));
        var service = new StudentService(repo);
        var before = await service.GetRosterAsync(1);

        var added = await service.AddAsync(1, Draft("Paz", "Ana", "200", "20", Today.AddDays(-2), "555", "ana@edu"), Today);

        var roster = await service.GetRosterAsync(1);
        Assert.Equal(before.Count + 1, roster.Count);
        Assert.Equal(["Alba", "Paz"], roster.Select(student => student.LastName));
        var ana = roster.Single(student => student.Dni == "20");
        Assert.Equal(added.Id, ana.Id);
        Assert.Equal("Ana", ana.FirstName);
        Assert.Equal("200", ana.StudentNumber);
        Assert.Equal("555", ana.Phone);
        Assert.Equal("ana@edu", ana.Email);
        Assert.Equal(Today.AddDays(-2), ana.EnrolledOn);
        Assert.DoesNotContain(await service.GetRosterAsync(2), student => student.Dni == "20");
    }

    [Fact]
    public async Task Given_TodayOrAnEarlierEnrollment_When_Adding_Then_ItIsAccepted()
    {
        var service = new StudentService(new FakeStudentRepository());

        var today = await service.AddAsync(1, Draft("Paz", "Ana", "1", "1", Today), Today);
        var earlier = await service.AddAsync(1, Draft("Rios", "Luis", "2", "2", Today.AddDays(-1)), Today);

        Assert.Equal(Today, today.EnrolledOn);
        Assert.Equal(Today.AddDays(-1), earlier.EnrolledOn);
    }

    [Theory]
    [InlineData("", "Ana", "1", "1", "apellido")]
    [InlineData("Paz", " ", "1", "1", "nombre")]
    [InlineData("Paz", "Ana", "", "1", "legajo")]
    [InlineData("Paz", "Ana", "1", "  ", "DNI")]
    public async Task Given_EmptyRequiredField_When_Adding_Then_NothingIsWritten(string last, string first, string number, string dni, string expected)
    {
        var repo = new FakeStudentRepository().WithSection(1, Existing("10", "100", "Alba", "Bea"));
        var service = new StudentService(repo);

        var error = await Assert.ThrowsAsync<StudentEnrollmentException>(() =>
            service.AddAsync(1, Draft(last, first, number, dni, Today), Today));

        Assert.Contains(expected, error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(await service.GetRosterAsync(1));
    }

    [Fact]
    public async Task Given_FutureEnrollment_When_Adding_Then_NothingIsWritten()
    {
        var service = new StudentService(new FakeStudentRepository());

        var error = await Assert.ThrowsAsync<StudentEnrollmentException>(() =>
            service.AddAsync(1, Draft("Paz", "Ana", "1", "1", Today.AddDays(1)), Today));

        Assert.Contains("posterior", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(await service.GetRosterAsync(1));
    }

    [Fact]
    public async Task Given_StudentNumberAlreadyInTheSection_When_Adding_Then_TheMessageNamesTheOwner()
    {
        var repo = new FakeStudentRepository().WithSection(1, Existing("10", "12345", "Paz", "Ana"));
        var service = new StudentService(repo);

        var error = await Assert.ThrowsAsync<StudentEnrollmentException>(() =>
            service.AddAsync(1, Draft("Rios", "Luis", "12345", "99", Today), Today));

        Assert.Contains("12345", error.Message);
        Assert.Contains("Paz, Ana", error.Message);
        Assert.Single(await service.GetRosterAsync(1));
    }

    [Fact]
    public async Task Given_DniAlreadyStoredInAnotherSection_When_Adding_Then_TheMessageNamesTheOwner()
    {
        var repo = new FakeStudentRepository()
            .WithSection(1, Existing("30111222", "100", "Paz", "Ana"))
            .WithSection(2);
        var service = new StudentService(repo);

        var error = await Assert.ThrowsAsync<StudentEnrollmentException>(() =>
            service.AddAsync(2, Draft("Rios", "Luis", "200", "30111222", Today), Today));

        Assert.Contains("30111222", error.Message);
        Assert.Contains("Paz, Ana", error.Message);
        Assert.Empty(await service.GetRosterAsync(2));
        Assert.Single(await service.GetRosterAsync(1));
    }

    private static Student Existing(string dni, string number, string last, string first) => new()
    {
        Dni = dni,
        StudentNumber = number,
        LastName = last,
        FirstName = first,
        Name = $"{first} {last}"
    };

    private static StudentDraft Draft(string last, string first, string number, string dni, DateTime enrolled, string? phone = null, string? email = null) =>
        new(last, first, number, dni, enrolled, phone, email);
}
