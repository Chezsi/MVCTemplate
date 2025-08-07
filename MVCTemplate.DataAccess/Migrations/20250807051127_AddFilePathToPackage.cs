using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFilePathToPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Packages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Packages");
        }
    }
}
