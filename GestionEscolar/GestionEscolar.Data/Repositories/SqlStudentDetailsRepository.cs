using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

public sealed class SqlStudentDetailsRepository : IStudentDetailsRepository
{
    private const string StudentSql = @"
SELECT s.id, s.first_name, s.last_name, s.student_number, s.phone, s.email, s.section_id, sec.name AS section_name
FROM dbo.student AS s
JOIN dbo.section AS sec ON sec.id = s.section_id
WHERE s.id = @studentId;";

    // LEFT JOIN: aparecen todas las clases de la comisión aunque no haya registro del alumno.
    private const string AttendanceSql = @"
SELECT cs.id AS class_session_id, cs.session_date, ar.id AS record_id, ar.status
FROM dbo.class_session AS cs
LEFT JOIN dbo.attendance_record AS ar
       ON ar.class_session_id = cs.id AND ar.student_id = @studentId
WHERE cs.section_id = @sectionId
ORDER BY cs.session_date DESC;";

    // LEFT JOIN: aparecen todos los trabajos de la comisión aunque no haya registro del alumno.
    private const string SubmissionsSql = @"
SELECT a.id AS assignment_id, a.name, a.due_date, sub.id AS submission_id, sub.status
FROM dbo.assignment AS a
LEFT JOIN dbo.submission AS sub
       ON sub.assignment_id = a.id AND sub.student_id = @studentId
WHERE a.section_id = @sectionId
ORDER BY a.due_date, a.id;";

    private const string InterventionsSql = @"
SELECT i.id, i.intervention_date, i.channel, i.outcome, i.note
FROM dbo.intervention AS i
WHERE i.student_id = @studentId
ORDER BY i.intervention_date DESC, i.id DESC;";

    private readonly DatabaseInstance _db;

    public SqlStudentDetailsRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<StudentDetail?> GetStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(StudentSql, conn);
        cmd.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return new StudentDetail
        {
            StudentId = reader.GetInt32(reader.GetOrdinal("id")),
            Name = reader.GetString(reader.GetOrdinal("first_name")),
            LastName = reader.GetString(reader.GetOrdinal("last_name")),
            StudentNumber = reader.GetString(reader.GetOrdinal("student_number")),
            PhoneNumber = GetStringOrEmpty(reader, "phone"),
            Email = GetStringOrEmpty(reader, "email"),
            SectionId = reader.GetInt32(reader.GetOrdinal("section_id")),
            SectionName = reader.GetString(reader.GetOrdinal("section_name"))
        };
    }

    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(int studentId, int sectionId, CancellationToken cancellationToken = default)
    {
        var records = new List<AttendanceRecord>();

        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(AttendanceSql, conn);
        cmd.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            records.Add(new AttendanceRecord
            {
                Id = GetNullableInt(reader, "record_id"),
                StudentId = studentId,
                ClassSessionId = reader.GetInt32(reader.GetOrdinal("class_session_id")),
                Date = reader.GetDateTime(reader.GetOrdinal("session_date")),
                Status = DbStatusValues.ParseAttendanceStatus(GetNullableString(reader, "status"))
            });
        }

        return records;
    }

    public async Task<IReadOnlyList<Submission>> GetSubmissionsAsync(int studentId, int sectionId, CancellationToken cancellationToken = default)
    {
        var submissions = new List<Submission>();

        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(SubmissionsSql, conn);
        cmd.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            submissions.Add(new Submission
            {
                Id = GetNullableInt(reader, "submission_id"),
                StudentId = studentId,
                SectionId = sectionId,
                AssignmentId = reader.GetInt32(reader.GetOrdinal("assignment_id")),
                AssignmentName = reader.GetString(reader.GetOrdinal("name")),
                DueDate = reader.GetDateTime(reader.GetOrdinal("due_date")),
                Status = DbStatusValues.ParseSubmissionStatus(GetNullableString(reader, "status"))
            });
        }

        return submissions;
    }

    public async Task<IReadOnlyList<Intervention>> GetInterventionsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var interventions = new List<Intervention>();

        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(InterventionsSql, conn);
        cmd.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            interventions.Add(new Intervention
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                StudentId = studentId,
                Date = reader.GetDateTime(reader.GetOrdinal("intervention_date")),
                Channel = reader.GetString(reader.GetOrdinal("channel")),
                Outcome = reader.GetString(reader.GetOrdinal("outcome")),
                Note = GetNullableString(reader, "note")
            });
        }

        return interventions;
    }

    private static string GetStringOrEmpty(SqlDataReader reader, string column) =>
        GetNullableString(reader, column) ?? string.Empty;

    private static string? GetNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? GetNullableInt(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }
}
