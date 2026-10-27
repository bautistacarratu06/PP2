using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEscolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class StudentSectionIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_student_section_id",
                schema: "dbo",
                table: "student",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_student_section_id",
                schema: "dbo",
                table: "student");
        }
    }
}
