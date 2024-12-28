using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeMicroservice.Functions.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Employees");

            migrationBuilder.AddColumn<int[]>(
                name: "Roles",
                table: "Employees",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles",
                table: "Employees");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
