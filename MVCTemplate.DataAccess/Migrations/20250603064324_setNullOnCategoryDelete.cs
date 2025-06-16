using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class setNullOnCategoryDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Persons",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons",
                column: "CategoryId",
                principalTable: "Categorys",
                principalColumn: "IdCategory",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons",
                column: "CategoryId",
                principalTable: "Categorys",
                principalColumn: "IdCategory",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
