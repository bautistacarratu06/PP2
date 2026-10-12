using GestionEscolar.Domain.Data;
using Microsoft.Data.SqlClient;
using UI.Models;

namespace UI.Services;

// Devuelve las filas de la grilla de Seguimiento para una comisión (section).
// El resultado es Student: fabricación pura para la UI (no es la tabla dbo.student),
// que después TrackingViewModel envuelve en StudentRow.
public static class GetTrakingRowsBySection
{
    // TODO: el nivel y el score de riesgo están hardcodeados por ahora.
    // Más adelante calcularlos con ScoringRiskServices o leerlos de dbo.vw_student_risk.
    private const string DefaultRiskLevel = "Medio";
    private const int DefaultRiskScore = 40;

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
      WHERE ar.student_id = s.id AND cs.section_id = s.section_id AND ar.status = N'presente') AS attended_classes,
    (SELECT COUNT(*) FROM dbo.assignment a WHERE a.section_id = s.section_id) AS total_assignments,
    (SELECT COUNT(*)
       FROM dbo.submission sub
       JOIN dbo.assignment a ON a.id = sub.assignment_id
      WHERE sub.student_id = s.id AND a.section_id = s.section_id AND sub.status = N'entregó') AS submitted_assignments,
    (SELECT MAX(i.intervention_date) FROM dbo.intervention i WHERE i.student_id = s.id) AS last_contact
FROM dbo.student AS s
WHERE s.section_id = @sectionId
ORDER BY s.last_name, s.first_name;";

    public static List<Student> GetRows(int sectionId)
    {
        var db = DatabaseInstance.GetInstance("MainConnection");
        var students = new List<Student>();

        using SqlConnection conn = db.GetConnection();
        conn.Open();

        using var cmd = new SqlCommand(Sql, conn);
        cmd.Parameters.Add("@sectionId", System.Data.SqlDbType.Int).Value = sectionId;

        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Name = reader.GetString(reader.GetOrdinal("full_name")),
                StudentNumber = reader.GetString(reader.GetOrdinal("student_number")),
                Phone = GetStringOrEmpty(reader, "phone"),
                Email = GetStringOrEmpty(reader, "email"),
                TotalClasses = reader.GetInt32(reader.GetOrdinal("total_classes")),
                AttendedClasses = reader.GetInt32(reader.GetOrdinal("attended_classes")),
                TotalAssignments = reader.GetInt32(reader.GetOrdinal("total_assignments")),
                SubmittedAssignments = reader.GetInt32(reader.GetOrdinal("submitted_assignments")),
                LastContact = reader.IsDBNull(reader.GetOrdinal("last_contact"))
                    ? null
                    : reader.GetDateTime(reader.GetOrdinal("last_contact")),
                RiskLevel = DefaultRiskLevel,
                RiskScore = DefaultRiskScore
            });
        }

        return students;
    }

    private static string GetStringOrEmpty(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}