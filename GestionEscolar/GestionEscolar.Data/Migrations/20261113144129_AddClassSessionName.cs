using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEscolar.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClassSessionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "name",
                schema: "dbo",
                table: "class_session",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "name",
                schema: "dbo",
                table: "class_session");
        }
    }
}
