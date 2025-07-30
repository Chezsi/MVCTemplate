using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RestrictSiteDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Sites_SiteId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Sites_SiteId",
                table: "Managers",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons",
                column: "CategoryId",
                principalTable: "Categorys",
                principalColumn: "IdCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Managers_Sites_SiteId",
                table: "Managers");

            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons");

            migrationBuilder.AddForeignKey(
                name: "FK_Managers_Sites_SiteId",
                table: "Managers",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons",
                column: "CategoryId",
                principalTable: "Categorys",
                principalColumn: "IdCategory",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
