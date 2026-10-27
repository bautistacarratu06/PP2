using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEscolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentEnrolledOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Los alumnos que ya estaban cargados usan el día, sin hora, de created_at.
            // Cada lote es idempotente: la prueba de esquema histórico vuelve a aplicar las migraciones.
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.student', N'enrolled_on') IS NULL
                    ALTER TABLE dbo.student ADD enrolled_on date NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE dbo.student
                SET enrolled_on = CAST(created_at AS date)
                WHERE enrolled_on IS NULL;
                """);

            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'dbo.student')
                      AND name = N'enrolled_on'
                      AND is_nullable = 1)
                    ALTER TABLE dbo.student ALTER COLUMN enrolled_on date NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.df_student_enrolled_on', N'D') IS NULL
                    ALTER TABLE dbo.student ADD CONSTRAINT df_student_enrolled_on
                        DEFAULT (CAST(SYSDATETIME() AS date)) FOR enrolled_on;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.df_student_enrolled_on', N'D') IS NOT NULL
                    ALTER TABLE dbo.student DROP CONSTRAINT df_student_enrolled_on;
                IF COL_LENGTH(N'dbo.student', N'enrolled_on') IS NOT NULL
                    ALTER TABLE dbo.student DROP COLUMN enrolled_on;
                """);
        }
    }
}
