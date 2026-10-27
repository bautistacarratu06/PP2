using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GestionEscolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El esquema histórico ya tiene dbo.section. En esa base este Up no crea,
            // no altera y no borra: SET NOEXEC saltea el lote y el cierre lo reanuda.
            migrationBuilder.Sql("IF OBJECT_ID(N'dbo.section', N'U') IS NOT NULL SET NOEXEC ON;");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "section",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_section", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "setting",
                schema: "dbo",
                columns: table => new
                {
                    setting_key = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    setting_value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_setting", x => x.setting_key);
                    table.CheckConstraint("ck_setting_value", "[setting_value] BETWEEN 0 AND 100");
                });

            migrationBuilder.CreateTable(
                name: "assignment",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment", x => x.id);
                    table.ForeignKey(
                        name: "fk_assignment_section",
                        column: x => x.section_id,
                        principalSchema: "dbo",
                        principalTable: "section",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "class_session",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    session_date = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_class_session", x => x.id);
                    table.ForeignKey(
                        name: "fk_class_session_section",
                        column: x => x.section_id,
                        principalSchema: "dbo",
                        principalTable: "section",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student",
                schema: "dbo",
                columns: table => new
                {
                    dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    student_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2(0)", nullable: false, defaultValueSql: "SYSDATETIME()")
                        .Annotation("Relational:DefaultConstraintName", "df_student_created_at")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student", x => x.dni);
                    table.UniqueConstraint("uq_student_id", x => x.id);
                    table.ForeignKey(
                        name: "fk_student_section",
                        column: x => x.section_id,
                        principalSchema: "dbo",
                        principalTable: "section",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "attendance_record",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_session_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    corrected_at = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance_record", x => x.id);
                    table.CheckConstraint("ck_attendance_record_status", "[status] IN (N'presente', N'ausente')");
                    table.ForeignKey(
                        name: "fk_attendance_record_class_session",
                        column: x => x.class_session_id,
                        principalSchema: "dbo",
                        principalTable: "class_session",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_attendance_record_student",
                        column: x => x.student_id,
                        principalSchema: "dbo",
                        principalTable: "student",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "intervention",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    intervention_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CAST(SYSDATETIME() AS date)")
                        .Annotation("Relational:DefaultConstraintName", "df_intervention_date"),
                    channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    outcome = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_intervention", x => x.id);
                    table.CheckConstraint("ck_intervention_channel", "[channel] IN (N'Llamada', N'WhatsApp', N'Correo', N'Presencial')");
                    table.CheckConstraint("ck_intervention_outcome", "[outcome] IN (N'Contactado - se comprometió a volver', N'Contactado - sin novedades', N'No contestó', N'Abandonó confirmado')");
                    table.ForeignKey(
                        name: "fk_intervention_student",
                        column: x => x.student_id,
                        principalSchema: "dbo",
                        principalTable: "student",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "submission",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assignment_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submission", x => x.id);
                    table.CheckConstraint("ck_submission_status", "[status] IN (N'entregó', N'no entregó')");
                    table.ForeignKey(
                        name: "fk_submission_assignment",
                        column: x => x.assignment_id,
                        principalSchema: "dbo",
                        principalTable: "assignment",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_submission_student",
                        column: x => x.student_id,
                        principalSchema: "dbo",
                        principalTable: "student",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "setting",
                columns: new[] { "setting_key", "setting_value" },
                values: new object[,]
                {
                    { "high_threshold", 50 },
                    { "medium_threshold", 25 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_assignment_section_id",
                schema: "dbo",
                table: "assignment",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_student_id",
                schema: "dbo",
                table: "attendance_record",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "uq_attendance_record_session_student",
                schema: "dbo",
                table: "attendance_record",
                columns: new[] { "class_session_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_class_session_section_date",
                schema: "dbo",
                table: "class_session",
                columns: new[] { "section_id", "session_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_intervention_student_id",
                schema: "dbo",
                table: "intervention",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "uq_section_name",
                schema: "dbo",
                table: "section",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_student_section_student_number",
                schema: "dbo",
                table: "student",
                columns: new[] { "section_id", "student_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_submission_student_id",
                schema: "dbo",
                table: "submission",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "uq_submission_assignment_student",
                schema: "dbo",
                table: "submission",
                columns: new[] { "assignment_id", "student_id" },
                unique: true);

            migrationBuilder.Sql("SET NOEXEC OFF;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance_record",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "intervention",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "setting",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "submission",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "class_session",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "assignment",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "student",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "section",
                schema: "dbo");
        }
    }
}
