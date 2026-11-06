using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEscolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: el test de schema histórico reaplica migraciones con la tabla ya presente.
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.student', N'is_active') IS NULL
                    ALTER TABLE dbo.student ADD is_active bit NOT NULL
                        CONSTRAINT df_student_is_active DEFAULT (CONVERT([bit],(1)));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.df_student_is_active', N'D') IS NOT NULL
                    ALTER TABLE dbo.student DROP CONSTRAINT df_student_is_active;
                IF COL_LENGTH(N'dbo.student', N'is_active') IS NOT NULL
                    ALTER TABLE dbo.student DROP COLUMN is_active;
                """);
        }
    }
}
