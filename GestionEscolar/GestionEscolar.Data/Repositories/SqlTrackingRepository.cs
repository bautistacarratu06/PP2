using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

// Implementación SQL de ITrackingRepository. Antes era UI/Services/GetTrakingRowsBySection.
// Solo trae datos; el riesgo lo calcula TrackingService (Core).
public sealed class SqlTrackingRepository : ITrackingRepository
{
    // Un registro por alumno de la comisión:
    // - clases: total de class_session de la comisión y cuántas tiene "presente".
    // - trabajos: total de assignment de la comisión y cuántos tiene "entregó".
    // - último contacto: la intervención más reciente.
    private const string Sql = @"
SELECT
    s.id,
    CONCAT(s.first_name, N' ', s.last_name) AS full_name,
    s.student_number,
    s.phone,
    s.email,
    (SELECT COUNT(*) FROM dbo.class_session cs WHERE cs.section_id = s.section_id) AS total_classes,
    (SELECT COUNT(*)
       FROM dbo.attendance_record ar
       JOIN dbo.class_session cs ON cs.id = ar.class_session_id
      WHERE ar.student_id = s.id AND cs.section_id = s.section_id AND ar.status = @presentStatus) AS attended_classes,
    (SELECT COUNT(*) FROM dbo.assignment a WHERE a.section_id = s.section_id) AS total_assignments,
    (SELECT COUNT(*)
       FROM dbo.submission sub
       JOIN dbo.assignment a ON a.id = sub.assignment_id
      WHERE sub.student_id = s.id AND a.section_id = s.section_id AND sub.status = @submittedStatus) AS submitted_assignments,
    (SELECT MAX(i.intervention_date) FROM dbo.intervention i WHERE i.student_id = s.id) AS last_contact
FROM dbo.student AS s
WHERE s.section_id = @sectionId
ORDER BY s.last_name, s.first_name;";

    private readonly DatabaseInstance _db;

    public SqlTrackingRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<StudentTrackingData>> GetRowsBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = new List<StudentTrackingData>();

        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(Sql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        cmd.Parameters.Add("@presentStatus", SqlDbType.NVarChar, 10).Value = AttendanceStatus.Presente.ToDbValue();
        cmd.Parameters.Add("@submittedStatus", SqlDbType.NVarChar, 12).Value = SubmissionStatus.Entrego.ToDbValue();

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add(new StudentTrackingData
            {
                Student = new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("full_name")),
                    StudentNumber = reader.GetString(reader.GetOrdinal("student_number")),
                    Phone = GetStringOrEmpty(reader, "phone"),
                    Email = GetStringOrEmpty(reader, "email")
                },
                TotalClasses = reader.GetInt32(reader.GetOrdinal("total_classes")),
                AttendedClasses = reader.GetInt32(reader.GetOrdinal("attended_classes")),
                TotalAssignments = reader.GetInt32(reader.GetOrdinal("total_assignments")),
                SubmittedAssignments = reader.GetInt32(reader.GetOrdinal("submitted_assignments")),
                LastContact = reader.IsDBNull(reader.GetOrdinal("last_contact"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("last_contact"))
            });
        }

        return rows;
    }

    private static string GetStringOrEmpty(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}
