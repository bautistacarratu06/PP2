using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

public sealed class SqlAttendanceRepository : IAttendanceRepository
{
    // Todos los alumnos de la comisión de la clase, con su registro si existe.
    private const string MarksSql = @"
SELECT s.id, CONCAT(s.last_name, N', ', s.first_name) AS student_name, ar.status
FROM dbo.class_session AS cs
JOIN dbo.student AS s ON s.section_id = cs.section_id
LEFT JOIN dbo.attendance_record AS ar ON ar.class_session_id = cs.id AND ar.student_id = s.id
WHERE cs.id = @classSessionId
ORDER BY s.last_name, s.first_name;";

    // Upsert por (class_session_id, student_id), que es UNIQUE (uq_attendance_record_session_student).
    // corrected_at marca que se corrigió un registro existente. Solo inserta si el alumno es de la comisión.
    private const string UpsertSql = @"
SET XACT_ABORT ON;
BEGIN TRAN;
UPDATE dbo.attendance_record WITH (UPDLOCK, SERIALIZABLE)
   SET status = @status, corrected_at = SYSDATETIME()
 WHERE class_session_id = @classSessionId AND student_id = @studentId AND status <> @status;
IF NOT EXISTS (SELECT 1 FROM dbo.attendance_record WITH (UPDLOCK, SERIALIZABLE)
               WHERE class_session_id = @classSessionId AND student_id = @studentId)
    INSERT INTO dbo.attendance_record (class_session_id, student_id, status)
    SELECT cs.id, s.id, @status
    FROM dbo.class_session AS cs
    JOIN dbo.student AS s ON s.section_id = cs.section_id
    WHERE cs.id = @classSessionId AND s.id = @studentId;
COMMIT;";

    private readonly DatabaseInstance _db;

    public SqlAttendanceRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<AttendanceMark>> GetMarksAsync(int classSessionId, CancellationToken cancellationToken = default)
    {
        var rows = new List<AttendanceMark>();
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(MarksSql, conn);
        cmd.Parameters.Add("@classSessionId", SqlDbType.Int).Value = classSessionId;
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add(new AttendanceMark(
                reader.GetInt32(0),
                reader.GetString(1),
                DbStatusValues.ParseAttendanceStatus(reader.IsDBNull(2) ? null : reader.GetString(2))));
        }
        return rows;
    }

    public async Task SaveAsync(int classSessionId, int studentId, AttendanceStatus status, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(UpsertSql, conn);
        cmd.Parameters.Add("@classSessionId", SqlDbType.Int).Value = classSessionId;
        cmd.Parameters.Add("@studentId", SqlDbType.Int).Value = studentId;
        cmd.Parameters.Add("@status", SqlDbType.NVarChar, 10).Value = status.ToDbValue();
        await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
