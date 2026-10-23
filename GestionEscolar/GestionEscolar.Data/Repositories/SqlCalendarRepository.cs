using System.Data;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

public sealed class SqlCalendarRepository : ICalendarRepository
{
    private const string SessionsSql = @"
SELECT id, section_id, session_date FROM dbo.class_session
WHERE section_id = @sectionId ORDER BY session_date;";

    private const string AssignmentsSql = @"
SELECT id, section_id, name, due_date FROM dbo.assignment
WHERE section_id = @sectionId ORDER BY due_date, name;";

    private const string InsertSessionSql = @"
INSERT INTO dbo.class_session (section_id, session_date) OUTPUT INSERTED.id VALUES (@sectionId, @date);";

    private const string InsertAssignmentSql = @"
INSERT INTO dbo.assignment (section_id, name, due_date) OUTPUT INSERTED.id VALUES (@sectionId, @name, @dueDate);";

    // Violación de UNIQUE (uq_class_session_section_date).
    private const int UniqueViolation = 2627;
    private const int UniqueIndexViolation = 2601;

    private readonly DatabaseInstance _db;

    public SqlCalendarRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<ClassSession>> GetClassSessionsAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = new List<ClassSession>();
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(SessionsSql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            rows.Add(new ClassSession(reader.GetInt32(0), reader.GetInt32(1), reader.GetDateTime(2)));
        return rows;
    }

    public async Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = new List<Assignment>();
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(AssignmentsSql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            rows.Add(new Assignment(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetDateTime(3)));
        return rows;
    }

    public async Task<int> AddClassSessionAsync(int sectionId, DateTime date, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(InsertSessionSql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        cmd.Parameters.Add("@date", SqlDbType.Date).Value = date.Date;
        try
        {
            return (int)(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false))!;
        }
        catch (SqlException ex) when (ex.Number is UniqueViolation or UniqueIndexViolation)
        {
            throw new CalendarValidationException($"Ya hay una clase el {date:dd/MM/yyyy} en esta comisión.");
        }
    }

    public async Task<int> AddAssignmentAsync(int sectionId, string name, DateTime dueDate, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(InsertAssignmentSql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 120).Value = name;
        cmd.Parameters.Add("@dueDate", SqlDbType.Date).Value = dueDate.Date;
        return (int)(await cmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false))!;
    }
}
