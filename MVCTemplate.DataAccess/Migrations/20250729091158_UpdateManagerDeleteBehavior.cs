using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateManagerDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Managers_ManagerId",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Managers_ManagerId",
                table: "Products",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Managers_ManagerId",
                table: "Products");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Managers_ManagerId",
                table: "Products",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull); // or whatever the old behavior was
        }

    }
}
