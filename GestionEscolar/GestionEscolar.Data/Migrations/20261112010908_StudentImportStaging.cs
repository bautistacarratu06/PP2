using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEscolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentImportStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabla de paso para la importación masiva (SqlBulkCopy). No es parte del modelo de EF:
            // cada importación usa su propio import_id y borra sus filas al terminar.
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.student_import_staging', N'U') IS NULL
                    CREATE TABLE dbo.student_import_staging
                    (
                        import_id      uniqueidentifier NOT NULL,
                        line           int              NOT NULL,
                        dni            varchar(20)      NOT NULL,
                        student_number nvarchar(20)     NOT NULL,
                        last_name      nvarchar(80)     NOT NULL,
                        first_name     nvarchar(80)     NOT NULL,
                        phone          nvarchar(30)     NULL,
                        email          nvarchar(120)    NULL,
                        enrolled_on    date             NOT NULL,
                        CONSTRAINT pk_student_import_staging PRIMARY KEY (import_id, line)
                    );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("IF OBJECT_ID(N'dbo.student_import_staging', N'U') IS NOT NULL DROP TABLE dbo.student_import_staging;");
        }
    }
}
