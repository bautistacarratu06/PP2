using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class StudentDetailsServiceTests
{
    private static FakeStudentDetailsRepository Repo()
    {
        var repo = new FakeStudentDetailsRepository();
        repo.Students[2] = new StudentDetail { StudentId = 2, Name = "Tomás", LastName = "González", SectionId = 1, SectionName = "Mañana - 1A" };
        repo.Students[15] = new StudentDetail { StudentId = 15, Name = "Catalina", LastName = "Acosta", SectionId = 1, SectionName = "Mañana - 1A" };
        repo.ClassSessionSection[10] = 1;
        repo.ClassSessionSection[20] = 2;
        repo.Attendance.Add(new AttendanceRecord { StudentId = 2, ClassSessionId = 10, Status = AttendanceStatus.Presente });
        repo.Attendance.Add(new AttendanceRecord { StudentId = 2, ClassSessionId = 20, Status = AttendanceStatus.Ausente });
        repo.Submissions.Add(new Submission { StudentId = 2, SectionId = 1, AssignmentName = "TP1", Status = SubmissionStatus.Entrego });
        repo.Interventions.Add(new Intervention { StudentId = 2, Channel = "Llamada", Outcome = "No contestó" });
        return repo;
    }

    [Fact]
    public async Task Given_RequestedSection_When_GettingDetail_Then_HistoriesBelongToThatSection()
    {
        var repo = Repo();
        var detail = await new StudentDetailsService(repo).GetAsync(2, sectionId: 1);

        Assert.NotNull(detail);
        Assert.Equal("Tomás González", detail.FullName);
        Assert.True(Assert.Single(detail.AttendanceRecords).IsPresent);
        Assert.True(Assert.Single(detail.Submissions).IsSubmitted);
        Assert.Single(detail.Interventions);
        Assert.Equal(1, repo.LastAttendanceSection);
        Assert.Equal(1, repo.LastSubmissionsSection);
    }

    [Fact]
    public async Task Given_NoSection_When_GettingDetail_Then_UsesStudentSection()
    {
        var repo = Repo();

        await new StudentDetailsService(repo).GetAsync(2);

        Assert.Equal(1, repo.LastAttendanceSection);
    }

    [Fact]
    public async Task Given_StudentWithoutRecords_When_GettingDetail_Then_ListsAreEmpty()
    {
        var detail = await new StudentDetailsService(Repo()).GetAsync(15, 1);

        Assert.NotNull(detail);
        Assert.Empty(detail.AttendanceRecords);
        Assert.Empty(detail.Submissions);
        Assert.Empty(detail.Interventions);
    }

    [Fact]
    public async Task Given_UnknownStudent_When_GettingDetail_Then_ReturnsNull()
    {
        Assert.Null(await new StudentDetailsService(Repo()).GetAsync(999, 1));
    }

    [Theory]
    [InlineData("presente", AttendanceStatus.Presente)]
    [InlineData("ausente", AttendanceStatus.Ausente)]
    [InlineData(null, null)]
    public void Given_AttendanceDbValue_When_Parsing_Then_MapsToEnum(string? db, AttendanceStatus? expected)
    {
        Assert.Equal(expected, DbStatusValues.ParseAttendanceStatus(db));
    }

    [Theory]
    [InlineData("entregó", SubmissionStatus.Entrego)]
    [InlineData("no entregó", SubmissionStatus.NoEntrego)]
    [InlineData(null, null)]
    public void Given_SubmissionDbValue_When_Parsing_Then_MapsToEnum(string? db, SubmissionStatus? expected)
    {
        Assert.Equal(expected, DbStatusValues.ParseSubmissionStatus(db));
    }

    [Fact]
    public void Given_InterventionWithNote_When_ReadingDescription_Then_IncludesNote()
    {
        var i = new Intervention { Outcome = "No contestó", Note = "reintentar" };
        Assert.Equal("No contestó (reintentar)", i.Description);
    }
}
