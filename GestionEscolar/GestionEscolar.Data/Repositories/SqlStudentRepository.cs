
using System.Data;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using Microsoft.Data.SqlClient;

namespace GestionEscolar.Data.Repositories;

// Solo trae datos de la DB.
public sealed class SqlStudentRepository : IStudentRepository
{
    // Un registro por alumno de la comisión.
    private const string Sql = @"
SELECT
    s.id,
    CONCAT(s.first_name, N' ', s.last_name) AS full_name,
    s.student_number,
    s.phone,
    s.email
FROM dbo.student AS s
WHERE s.section_id = @sectionId
ORDER BY s.last_name, s.first_name;";

    private readonly DatabaseInstance _db;

    public SqlStudentRepository(DatabaseInstance db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<Student>> GetRosterAsync(
        int sectionId,
        CancellationToken cancellationToken = default)
    {
        var rows = new List<Student>();

        await using var conn = await _db
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        await using var cmd = new SqlCommand(Sql, conn);
        cmd.Parameters.Add("@sectionId", SqlDbType.Int).Value = sectionId;
        cmd.Parameters.Add("@presentStatus", SqlDbType.NVarChar, 10).Value =
            AttendanceStatus.Presente.ToDbValue();
        cmd.Parameters.Add("@submittedStatus", SqlDbType.NVarChar, 12).Value =
            SubmissionStatus.Entrego.ToDbValue();

        await using var reader = await cmd
            .ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            rows.Add(
                new Student
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Name = reader.GetString(reader.GetOrdinal("full_name")),
                    StudentNumber = reader.GetString(reader.GetOrdinal("student_number")),
                    Phone = GetStringOrEmpty(reader, "phone"),
                    Email = GetStringOrEmpty(reader, "email")
                }
            );
        }

        return rows;
    }

    private static string GetStringOrEmpty(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}
