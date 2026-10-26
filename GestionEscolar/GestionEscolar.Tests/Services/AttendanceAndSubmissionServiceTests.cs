using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class AttendanceAndSubmissionServiceTests
{
    private static readonly DateTime Today = new(2026, 9, 10);

    [Fact]
    public async Task Given_AttendanceSaved_When_SavingSameStudentAgain_Then_RecordIsUpdated()
    {
        var att = new FakeAttendanceRepository();
        att.StudentsBySession[1] = [(5, "Acosta, Elena"), (6, "Gómez, Ana")];
        var service = new AttendanceService(new FakeCalendarRepository(), att);

        await service.SaveAsync(1, 5, AttendanceStatus.Presente);
        await service.SaveAsync(1, 5, AttendanceStatus.Ausente); // corrige, no duplica

        var marks = await service.GetMarksAsync(1);
        Assert.Single(att.Records);
        Assert.Equal(AttendanceStatus.Ausente, marks[0].Status);
        Assert.Null(marks[1].Status);
    }

    [Fact]
    public async Task Given_ClassSessions_When_Listing_Then_OrderedByDate()
    {
        var cal = new FakeCalendarRepository();
        cal.Sessions.Add(new ClassSession(2, 1, Today));
        cal.Sessions.Add(new ClassSession(1, 1, Today.AddDays(-7)));

        var sessions = await new AttendanceService(cal, new FakeAttendanceRepository()).GetClassSessionsAsync(1);

        Assert.Equal(new[] { 1, 2 }, sessions.Select(s => s.Id));
    }

    [Fact]
    public void Given_ClassSessions_When_PickingDefault_Then_LatestUpToTodayElseFirst()
    {
        var past = new ClassSession(1, 1, Today.AddDays(-1));
        var future = new ClassSession(2, 1, Today.AddDays(5));

        Assert.Equal(past, AttendanceService.PickDefault([future, past], Today));
        Assert.Equal(future, AttendanceService.PickDefault([future], Today));
        Assert.Null(AttendanceService.PickDefault([], Today));
    }

    [Fact]
    public async Task Given_SubmissionSaved_When_SavingSameStudentAgain_Then_RecordIsUpdated()
    {
        var sub = new FakeSubmissionRepository();
        sub.StudentsByAssignment[10] = [(5, "Acosta, Elena")];
        var service = new SubmissionService(new FakeCalendarRepository(), sub);

        await service.SaveAsync(10, 5, SubmissionStatus.NoEntrego);
        await service.SaveAsync(10, 5, SubmissionStatus.Entrego);

        Assert.Equal(SubmissionStatus.Entrego, Assert.Single(await service.GetMarksAsync(10)).Status);
        Assert.Single(sub.Records);
    }

    [Fact]
    public void Given_PastTodayAndFutureDueDates_When_PickingDefault_Then_LatestDueUpToToday()
    {
        var a1 = new Assignment(1, 1, "TP1", Today.AddDays(-5));
        var a2 = new Assignment(2, 1, "TP2", Today);
        var a3 = new Assignment(3, 1, "TP3", Today.AddDays(3));

        Assert.Equal(a2, SubmissionService.PickDefault([a3, a1, a2], Today));
    }

    [Fact]
    public void Given_TodayAndFutureAssignments_When_PickingDefault_Then_TodayIsChosen()
    {
        var today = new Assignment(2, 1, "Hoy", Today);
        var future = new Assignment(3, 1, "Futuro", Today.AddDays(3));

        Assert.Equal(today, SubmissionService.PickDefault([future, today], Today));
    }

    [Fact]
    public async Task Given_AssignmentsOutOfOrder_When_Listing_Then_OrderedByDueDateThenName()
    {
        var cal = new FakeCalendarRepository();
        var sameDay = Today;
        var later = Today.AddDays(4);
        cal.Assignments.Add(new Assignment(3, 1, "Zeta", later));
        cal.Assignments.Add(new Assignment(2, 1, "Beta", sameDay));
        cal.Assignments.Add(new Assignment(1, 1, "Alfa", sameDay));
        cal.Assignments.Add(new Assignment(9, 2, "Otra comisión", sameDay));

        var assignments = await new SubmissionService(cal, new FakeSubmissionRepository()).GetAssignmentsAsync(1);

        Assert.Equal(new[] { "Alfa", "Beta", "Zeta" }, assignments.Select(a => a.Name));
    }

    [Fact]
    public void Given_OnlyFutureAssignments_When_PickingDefault_Then_FirstByDate()
    {
        var later = new Assignment(2, 1, "Después", Today.AddDays(5));
        var sooner = new Assignment(1, 1, "Antes", Today.AddDays(1));

        Assert.Equal(sooner, SubmissionService.PickDefault([later, sooner], Today));
    }

    [Fact]
    public void Given_NoAssignments_When_PickingDefault_Then_ResultIsEmpty()
    {
        Assert.Null(SubmissionService.PickDefault([], Today));
    }
}
