using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ManagerId",
                table: "Products",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Managers_ManagerId",
                table: "Products",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Managers_ManagerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_ManagerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Products");
        }
    }
}
