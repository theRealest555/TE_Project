using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TE_Project.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFullNameAddGenderEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Submissions");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "Submissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Submissions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "Submissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "Submissions");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Submissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
