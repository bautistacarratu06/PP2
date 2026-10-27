using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Data.Persistence;
using GestionEscolar.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GestionEscolar.Data.Tests;

[Collection("sqlserver")]
public sealed class CourseTrackingDatabaseTests(SqlServerFixture fixture)
{
    private readonly SqlServerFixture _fixture = fixture;

    [Fact]
    public async Task Given_MigratedDatabase_When_InspectingSchema_Then_ThresholdsAndIndexExistWithoutTheView()
    {
        Assert.Equal(50, await ScalarAsync<int>("SELECT setting_value FROM dbo.setting WHERE setting_key = N'high_threshold'"));
        Assert.Equal(25, await ScalarAsync<int>("SELECT setting_value FROM dbo.setting WHERE setting_key = N'medium_threshold'"));
        Assert.Equal(1, await ScalarAsync<int>("SELECT COUNT(*) FROM dbo.setting WHERE setting_key = N'high_threshold'"));
        Assert.Null(await ScalarAsync<object>("SELECT OBJECT_ID(N'dbo.vw_student_risk', N'V')"));
        Assert.NotNull(await ScalarAsync<object>(
            "SELECT 1 FROM sys.indexes WHERE name = N'ix_student_section_id' AND object_id = OBJECT_ID(N'dbo.student')"));
        Assert.Equal("dni", await ScalarAsync<string>(
            """
            SELECT c.name
            FROM sys.indexes AS i
            JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
            JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
            WHERE i.is_primary_key = 1 AND i.object_id = OBJECT_ID(N'dbo.student')
            """));
    }

    [Fact]
    public async Task Given_TwoSections_When_Listing_Then_TheyComeBackOrderedById()
    {
        await _fixture.ClearRowsAsync();
        await using var db = _fixture.CreateContext();
        db.Sections.Add(new SectionEntity { Name = "Tarde" });
        db.Sections.Add(new SectionEntity { Name = "Manana" });
        await db.SaveChangesAsync();

        var sections = await _fixture.Get<ISectionRepository>().GetAllAsync();

        Assert.Equal(new[] { "Tarde", "Manana" }, sections.OrderBy(s => s.Id).Select(s => s.Name));
        Assert.Equal(sections.Select(s => s.Id).OrderBy(id => id), sections.Select(s => s.Id));
    }

    [Fact]
    public async Task Given_Roster_When_ReadingASection_Then_NamesAreFirstThenLastAndOrderedByLastName()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Nomina");
        await using (var db = _fixture.CreateContext())
        {
            db.Students.Add(Student(sectionId, "20", "200", "Zulu", "Ana", "111", "ana@x.test"));
            db.Students.Add(Student(sectionId, "10", "100", "Alba", "Bea", null, null));
            await db.SaveChangesAsync();
        }

        var roster = await _fixture.Get<IStudentRepository>().GetRosterAsync(sectionId);

        Assert.Equal(["Bea Alba", "Ana Zulu"], roster.Select(s => s.Name));
        Assert.Equal("100", roster[0].StudentNumber);
        Assert.Equal(string.Empty, roster[0].Phone);
        Assert.Equal(string.Empty, roster[0].Email);
        Assert.Equal("111", roster[1].Phone);
        Assert.Empty(await _fixture.Get<IStudentRepository>().GetRosterAsync(await SeedSectionAsync("Vacia")));
    }

    [Fact]
    public async Task Given_CalendarRules_When_AddingClassesAndAssignments_Then_DuplicatesAndBlankNamesAreRejected()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Calendario");
        var calendar = new CalendarService(_fixture.Get<ICalendarRepository>());
        var day = new DateTime(2026, 5, 4);

        await calendar.AddClassAsync(sectionId, day.AddHours(15));
        var duplicate = await Assert.ThrowsAsync<CalendarValidationException>(() => calendar.AddClassAsync(sectionId, day));
        Assert.Contains("04/05/2026", duplicate.Message);
        Assert.Equal(1, await ScalarAsync<int>($"SELECT COUNT(*) FROM dbo.class_session WHERE section_id = {sectionId}"));

        await Assert.ThrowsAsync<CalendarValidationException>(() => calendar.AddAssignmentAsync(sectionId, "  ", day));
        await Assert.ThrowsAsync<CalendarValidationException>(() => calendar.AddAssignmentAsync(sectionId, new string('a', 121), day));
        await calendar.AddAssignmentAsync(sectionId, "  TP 1  ", day.AddHours(8));

        var items = await calendar.GetItemsAsync(sectionId);
        Assert.Equal([CalendarItemKind.Clase, CalendarItemKind.Trabajo], items.Select(i => i.Kind));
        Assert.Equal("TP 1", items[1].Name);
        Assert.Equal(day.Date, items[1].Date);
    }

    [Fact]
    public async Task Given_ClassUniqueIndex_When_InsertingTheSameDayTwice_Then_RepositoryRejectsTheSecond()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Unique");
        var repository = _fixture.Get<ICalendarRepository>();
        var day = new DateTime(2026, 6, 1, 22, 0, 0);

        await repository.AddClassSessionAsync(sectionId, day);
        var error = await Assert.ThrowsAsync<CalendarValidationException>(() => repository.AddClassSessionAsync(sectionId, day));

        Assert.Contains("01/06/2026", error.Message);
        Assert.Equal(1, await ScalarAsync<int>($"SELECT COUNT(*) FROM dbo.class_session WHERE section_id = {sectionId}"));
    }

    [Fact]
    public async Task Given_Attendance_When_Saving_Then_MissingStudentsStayBlankAndCorrectionsStampTheClock()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Asistencia");
        var otherId = await SeedSectionAsync("Otra");
        int classId;
        int anaId;
        int outsiderId;
        await using (var db = _fixture.CreateContext())
        {
            db.Students.Add(Student(sectionId, "1", "1", "Paz", "Ana", null, null));
            db.Students.Add(Student(sectionId, "2", "2", "Rios", "Luis", null, null));
            db.Students.Add(Student(otherId, "3", "3", "Sosa", "Eva", null, null));
            db.ClassSessions.Add(new ClassSessionEntity { SectionId = sectionId, SessionDate = new DateTime(2026, 4, 1) });
            await db.SaveChangesAsync();
            classId = db.ClassSessions.Single().Id;
            anaId = db.Students.Single(s => s.Dni == "1").Id;
            outsiderId = db.Students.Single(s => s.Dni == "3").Id;
        }

        var attendance = _fixture.Get<IAttendanceRepository>();
        var before = await attendance.GetMarksAsync(classId);
        Assert.Equal(["Paz, Ana", "Rios, Luis"], before.Select(m => m.StudentName));
        Assert.All(before, mark => Assert.Null(mark.Status));

        await attendance.SaveAsync(classId, anaId, AttendanceStatus.Presente);
        Assert.Null(await CorrectedAtAsync(classId, anaId));

        await attendance.SaveAsync(classId, anaId, AttendanceStatus.Presente);
        Assert.Null(await CorrectedAtAsync(classId, anaId));

        await attendance.SaveAsync(classId, anaId, AttendanceStatus.Ausente);
        var corrected = await CorrectedAtAsync(classId, anaId);
        Assert.NotNull(corrected);

        await attendance.SaveAsync(classId, anaId, AttendanceStatus.Ausente);
        Assert.Equal(corrected, await CorrectedAtAsync(classId, anaId));

        await attendance.SaveAsync(classId, outsiderId, AttendanceStatus.Presente);
        Assert.Equal(1, await ScalarAsync<int>("SELECT COUNT(*) FROM dbo.attendance_record"));

        var after = await attendance.GetMarksAsync(classId);
        Assert.Equal(AttendanceStatus.Ausente, after.Single(m => m.StudentId == anaId).Status);
        Assert.Null(after.Single(m => m.StudentName == "Rios, Luis").Status);
    }

    [Fact]
    public async Task Given_Submissions_When_Saving_Then_OnlyTheEnrolledStudentGetsOneRow()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Entregas");
        int assignmentId;
        int studentId;
        int outsiderId;
        await using (var db = _fixture.CreateContext())
        {
            var other = new SectionEntity { Name = "Fuera" };
            db.Sections.Add(other);
            db.Students.Add(Student(sectionId, "9", "9", "Diaz", "Leo", null, null));
            db.Assignments.Add(new AssignmentEntity { SectionId = sectionId, Name = "TP", DueDate = new DateTime(2026, 7, 2) });
            await db.SaveChangesAsync();
            db.Students.Add(Student(other.Id, "8", "8", "Costa", "Lia", null, null));
            await db.SaveChangesAsync();
            assignmentId = db.Assignments.Single().Id;
            studentId = db.Students.Single(s => s.Dni == "9").Id;
            outsiderId = db.Students.Single(s => s.Dni == "8").Id;
        }

        var submissions = _fixture.Get<ISubmissionRepository>();
        var marks = await submissions.GetMarksAsync(assignmentId);
        Assert.Equal(["Diaz, Leo"], marks.Select(m => m.StudentName));
        Assert.Null(marks[0].Status);

        await submissions.SaveAsync(assignmentId, studentId, SubmissionStatus.NoEntrego);
        await submissions.SaveAsync(assignmentId, studentId, SubmissionStatus.Entrego);
        await submissions.SaveAsync(assignmentId, outsiderId, SubmissionStatus.Entrego);

        Assert.Equal(1, await ScalarAsync<int>("SELECT COUNT(*) FROM dbo.submission"));
        Assert.Equal(
            SubmissionStatus.Entrego,
            (await submissions.GetMarksAsync(assignmentId)).Single().Status);
    }

    [Fact]
    public async Task Given_StudentHistory_When_OpeningTheDetail_Then_MissingRowsStayAndOrderIsKept()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Ficha");
        int studentId;
        await using (var db = _fixture.CreateContext())
        {
            var student = Student(sectionId, "44", "44", "Nunez", "Sol", "555", "sol@x.test");
            db.Students.Add(student);
            db.ClassSessions.Add(new ClassSessionEntity { SectionId = sectionId, SessionDate = new DateTime(2026, 3, 1) });
            db.ClassSessions.Add(new ClassSessionEntity { SectionId = sectionId, SessionDate = new DateTime(2026, 3, 10) });
            db.Assignments.Add(new AssignmentEntity { SectionId = sectionId, Name = "Segundo", DueDate = new DateTime(2026, 4, 2) });
            db.Assignments.Add(new AssignmentEntity { SectionId = sectionId, Name = "Primero", DueDate = new DateTime(2026, 4, 1) });
            await db.SaveChangesAsync();
            studentId = student.Id;
            var latestClass = db.ClassSessions.Single(s => s.SessionDate == new DateTime(2026, 3, 10));
            db.AttendanceRecords.Add(new AttendanceRecordEntity
            {
                ClassSessionId = latestClass.Id,
                StudentId = studentId,
                Status = AttendanceStatus.Presente.ToDbValue()
            });
            var firstAssignment = db.Assignments.Single(a => a.Name == "Primero");
            db.Submissions.Add(new SubmissionEntity
            {
                AssignmentId = firstAssignment.Id,
                StudentId = studentId,
                Status = SubmissionStatus.Entrego.ToDbValue()
            });
            db.Interventions.Add(new InterventionEntity
            {
                StudentId = studentId,
                InterventionDate = new DateTime(2026, 5, 1),
                Channel = "Correo",
                Outcome = "No contestó",
                Note = "primero"
            });
            db.Interventions.Add(new InterventionEntity
            {
                StudentId = studentId,
                InterventionDate = new DateTime(2026, 5, 1),
                Channel = "Llamada",
                Outcome = "No contestó",
                Note = "segundo"
            });
            await db.SaveChangesAsync();
        }

        var details = _fixture.Get<IStudentDetailsRepository>();
        Assert.Null(await details.GetStudentAsync(studentId + 100));

        var detail = await details.GetStudentAsync(studentId);
        Assert.NotNull(detail);
        Assert.Equal("Sol", detail.Name);
        Assert.Equal("Nunez", detail.LastName);
        Assert.Equal("Ficha", detail.SectionName);
        Assert.Equal("555", detail.PhoneNumber);

        var attendance = await details.GetAttendanceRecordsAsync(studentId, sectionId);
        Assert.Equal(new DateTime(2026, 3, 10), attendance[0].Date);
        Assert.Equal(AttendanceStatus.Presente, attendance[0].Status);
        Assert.Null(attendance[1].Status);
        Assert.Null(attendance[1].Id);

        var submissions = await details.GetSubmissionsAsync(studentId, sectionId);
        Assert.Equal(["Primero", "Segundo"], submissions.Select(s => s.AssignmentName));
        Assert.Equal(SubmissionStatus.Entrego, submissions[0].Status);
        Assert.Null(submissions[1].Status);

        var interventions = await details.GetInterventionsAsync(studentId);
        Assert.Equal(["segundo", "primero"], interventions.Select(i => i.Note));
    }

    [Fact]
    public async Task Given_TrackingRows_When_Summarizing_Then_PercentsAndRiskMatchTheCoreRule()
    {
        await _fixture.ClearRowsAsync();
        var sectionId = await SeedSectionAsync("Seguimiento");
        await using (var db = _fixture.CreateContext())
        {
            var student = Student(sectionId, "7", "7", "Costa", "Ian", null, null);
            db.Students.Add(student);
            db.ClassSessions.Add(new ClassSessionEntity { SectionId = sectionId, SessionDate = new DateTime(2026, 8, 1) });
            db.ClassSessions.Add(new ClassSessionEntity { SectionId = sectionId, SessionDate = new DateTime(2026, 8, 2) });
            db.Assignments.Add(new AssignmentEntity { SectionId = sectionId, Name = "TP", DueDate = new DateTime(2026, 8, 3) });
            await db.SaveChangesAsync();
            db.AttendanceRecords.Add(new AttendanceRecordEntity
            {
                ClassSessionId = db.ClassSessions.OrderBy(s => s.SessionDate).First().Id,
                StudentId = student.Id,
                Status = AttendanceStatus.Presente.ToDbValue()
            });
            db.Submissions.Add(new SubmissionEntity
            {
                AssignmentId = db.Assignments.Single().Id,
                StudentId = student.Id,
                Status = SubmissionStatus.Entrego.ToDbValue()
            });
            db.Interventions.Add(new InterventionEntity
            {
                StudentId = student.Id,
                InterventionDate = new DateTime(2026, 8, 9),
                Channel = "WhatsApp",
                Outcome = "Contactado - sin novedades"
            });
            await db.SaveChangesAsync();
        }

        var rows = await _fixture.Get<ITrackingRepository>().GetRowsBySectionAsync(sectionId);
        var row = Assert.Single(rows);
        Assert.Equal("Ian Costa", row.Student.Name);
        Assert.Equal(2, row.TotalClasses);
        Assert.Equal(1, row.AttendedClasses);
        Assert.Equal(50, row.AttendancePercent);
        Assert.Equal(1, row.TotalAssignments);
        Assert.Equal(1, row.SubmittedAssignments);
        Assert.Equal(100, row.DeliveryPercent);
        Assert.Equal(new DateTime(2026, 8, 9), row.LastContact);

        var evaluation = RiskRule.Evaluate(1, 2, 1, 1, new RiskConfig());
        var summary = await _fixture.Get<IRiskSummaryRepository>().GetSummaryBySectionAsync(sectionId);
        var expected = RiskSummary.FromResults([new StudentRiskResult(row, evaluation.Score, evaluation.Level)]);
        Assert.Equal(expected, summary);

        var emptyId = await SeedSectionAsync("SinDatos");
        await using (var db = _fixture.CreateContext())
        {
            db.Students.Add(Student(emptyId, "70", "70", "Vacio", "Leo", null, null));
            await db.SaveChangesAsync();
        }

        var empty = Assert.Single(await _fixture.Get<ITrackingRepository>().GetRowsBySectionAsync(emptyId));
        Assert.Equal(0, empty.AttendancePercent);
        Assert.Equal(0, empty.DeliveryPercent);
        Assert.Null(empty.LastContact);
    }

    [Fact]
    public async Task Given_StoredThresholds_When_Saving_Then_BothKeysChangeTogether()
    {
        var settings = _fixture.Get<IRiskSettingsRepository>();
        var original = await settings.GetAsync();
        Assert.Equal(50, original.HighThreshold);
        Assert.Equal(25, original.MediumThreshold);

        try
        {
            await settings.SaveAsync(60, 30);
            var updated = await settings.GetAsync();
            Assert.Equal(60, updated.HighThreshold);
            Assert.Equal(30, updated.MediumThreshold);
        }
        finally
        {
            await settings.SaveAsync(50, 25);
        }
    }

    private async Task<int> SeedSectionAsync(string name)
    {
        await using var db = _fixture.CreateContext();
        var section = new SectionEntity { Name = name };
        db.Sections.Add(section);
        await db.SaveChangesAsync();
        return section.Id;
    }

    private static StudentEntity Student(
        int sectionId,
        string dni,
        string studentNumber,
        string lastName,
        string firstName,
        string? phone,
        string? email) => new()
        {
            SectionId = sectionId,
            Dni = dni,
            StudentNumber = studentNumber,
            LastName = lastName,
            FirstName = firstName,
            Phone = phone,
            Email = email
        };

    private async Task<DateTime?> CorrectedAtAsync(int classSessionId, int studentId)
    {
        await using var db = _fixture.CreateContext();
        return await db.AttendanceRecords
            .Where(record => record.ClassSessionId == classSessionId && record.StudentId == studentId)
            .Select(record => record.CorrectedAt)
            .SingleAsync();
    }

    private async Task<T?> ScalarAsync<T>(string sql)
    {
        await using var connection = new SqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var value = await command.ExecuteScalarAsync();
        if (value is null or DBNull)
            return default;
        return (T)value;
    }
}
