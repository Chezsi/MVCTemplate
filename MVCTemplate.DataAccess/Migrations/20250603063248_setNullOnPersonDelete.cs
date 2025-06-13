using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class setNullOnPersonDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Contracts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
