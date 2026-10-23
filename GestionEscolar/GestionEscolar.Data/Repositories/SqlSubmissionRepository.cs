using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

public sealed class SqlSubmissionRepository : ISubmissionRepository
{
    // Todos los alumnos de la comisión del trabajo, con su entrega si existe.
    private const string MarksSql = @"
SELECT s.id, CONCAT(s.last_name, N', ', s.first_name) AS student_name, sub.status
FROM dbo.assignment AS a
JOIN dbo.student AS s ON s.section_id = a.section_id
LEFT JOIN dbo.submission AS sub ON sub.assignment_id = a.id AND sub.student_id = s.id
WHERE a.id = @assignmentId
ORDER BY s.last_name, s.first_name;";

    // Upsert por (assignment_id, student_id), que es UNIQUE (uq_submission_assignment_student).
    // Solo inserta si el alumno es de la comisión del trabajo.
    private const string UpsertSql = @"
SET XACT_ABORT ON;
BEGIN TRAN;
UPDATE dbo.submission WITH (UPDLOCK, SERIALIZABLE)
   SET status = @status
 WHERE assignment_id = @assignmentId AND student_id = @studentId;
IF @@ROWCOUNT = 0
    INSERT INTO dbo.submission (assignment_id, student_id, status)
    SELECT a.id, s.id, @status
    FROM dbo.assignment AS a
    JOIN dbo.student AS s ON s.section_id = a.section_id
    WHERE a.id = @assignmentId AND s.id = @studentId;
COMMIT;";

    private readonly DatabaseInstance _db;

    public SqlSubmissionRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<SubmissionMark>> GetMarksAsync(int assignmentId, CancellationToken cancellationToken = default)
    {
        var rows = new List<SubmissionMark>();
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(MarksSql, conn);
        cmd.Parameters.Add("@assignmentId", SqlDbType.Int).Value = assignmentId;
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add(new SubmissionMark(
                reader.GetInt32(0),
                reader.GetString(1),
                DbStatusValues.ParseSubmissionStatus(reader.IsDBNull(2) ? null : reader.GetString(2))));
        }
        return rows;
    }

    public async Task SaveAsync(int assignmentId, int studentId, SubmissionStatus status, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(UpsertSql, conn);
        cmd.Parameters.Add("@assignmentId", SqlDbType.Int).Value = assignmentId;
        cmd.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;
        cmd.Parameters.Add("@status", SqlDbType.NVarChar, 12).Value = status.ToDbValue();
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
