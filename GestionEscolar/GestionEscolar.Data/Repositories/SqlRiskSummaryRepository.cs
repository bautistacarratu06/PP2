using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

// Cuenta alumnos por nivel de riesgo directamente en SQL.
//
// IMPORTANTE: replica la fórmula de RiskRule (Core) para que las tarjetas
// coincidan con la grilla:
//   riesgo = ROUND(((totalClases - asistidas) / totalClases * 0.6
//                 + (totalTrabajos - entregados) / totalTrabajos * 0.4) * 100)
//   - si un total es 0, esa parte vale 0;
//   - asistidas/entregados se recortan al total (igual que TrackingService);
//   - se calcula en FLOAT (IEEE double, como C#) y ROUND redondea alejándose de cero
//     (= MidpointRounding.AwayFromZero).
// Los umbrales llegan como parámetros desde RiskConfig (no están hardcodeados en la query).
// No usa dbo.vw_student_risk porque esa vista calcula sobre los registros cargados del alumno,
// no sobre el total de clases/trabajos de la comisión, y daría otros números.
// Si cambia la fórmula en RiskRule, hay que cambiarla acá también.
public sealed class SqlRiskSummaryRepository : IRiskSummaryRepository
{
    public const string Sql = @"
WITH totals AS (
    SELECT
        (SELECT COUNT(*) FROM dbo.class_session cs WHERE cs.section_id = @sectionId) AS total_classes,
        (SELECT COUNT(*) FROM dbo.assignment a WHERE a.section_id = @sectionId) AS total_assignments
), per_student AS (
    SELECT
        s.id,
        (SELECT COUNT(*)
           FROM dbo.attendance_record ar
           JOIN dbo.class_session cs ON cs.id = ar.class_session_id
          WHERE ar.student_id = s.id AND cs.section_id = @sectionId AND ar.status = @presentStatus) AS attended,
        (SELECT COUNT(*)
           FROM dbo.submission sub
           JOIN dbo.assignment a ON a.id = sub.assignment_id
          WHERE sub.student_id = s.id AND a.section_id = @sectionId AND sub.status = @submittedStatus) AS submitted
    FROM dbo.student AS s
    WHERE s.section_id = @sectionId
), scored AS (
    SELECT
        p.id,
        CAST(ROUND((
            CASE WHEN t.total_classes = 0 THEN 0.0E0
                 ELSE CAST(t.total_classes - IIF(p.attended > t.total_classes, t.total_classes, p.attended) AS FLOAT) / t.total_classes
            END * 0.6E0
          + CASE WHEN t.total_assignments = 0 THEN 0.0E0
                 ELSE CAST(t.total_assignments - IIF(p.submitted > t.total_assignments, t.total_assignments, p.submitted) AS FLOAT) / t.total_assignments
            END * 0.4E0
        ) * 100.0E0, 0) AS INT) AS risk
    FROM per_student AS p
    CROSS JOIN totals AS t
)
SELECT
    COUNT(CASE WHEN risk >= @highThreshold THEN 1 END) AS high_count,
    COUNT(CASE WHEN risk >= @mediumThreshold AND risk < @highThreshold THEN 1 END) AS medium_count,
    COUNT(CASE WHEN risk < @mediumThreshold THEN 1 END) AS low_count
FROM scored;";

    private readonly DatabaseInstance _db;
    private readonly RiskConfig _config;

    public SqlRiskSummaryRepository(DatabaseInstance db, RiskConfig config)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<RiskSummary> GetSummaryBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _db.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        await using var cmd = new SqlCommand(Sql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        cmd.Parameters.Add("@presentStatus", SqlDbType.NVarChar, 10).Value = AttendanceStatus.Presente.ToDbValue();
        cmd.Parameters.Add("@submittedStatus", SqlDbType.NVarChar, 12).Value = SubmissionStatus.Entrego.ToDbValue();
        cmd.Parameters.Add("@highThreshold", SqlDbType.Int).Value = _config.HighThreshold;
        cmd.Parameters.Add("@mediumThreshold", SqlDbType.Int).Value = _config.MediumThreshold;

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return RiskSummary.Empty;

        return new RiskSummary(
            reader.GetInt32(reader.GetOrdinal("high_count")),
            reader.GetInt32(reader.GetOrdinal("medium_count")),
            reader.GetInt32(reader.GetOrdinal("low_count")));
    }
}
