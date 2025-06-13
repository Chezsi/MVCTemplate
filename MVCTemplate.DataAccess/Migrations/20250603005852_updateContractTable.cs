using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateContractTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Alter columns to be nullable
            migrationBuilder.AlterColumn<DateTime>(
                name: "Validity",
                table: "Contracts",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Contracts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Drop existing foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts");

            // Recreate foreign key with ReferentialAction.NoAction
            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert foreign key to ReferentialAction.Restrict
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Persons_PersonId",
                table: "Contracts",
                column: "PersonId",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            // Revert columns to non-nullable with default values
            migrationBuilder.AlterColumn<DateTime>(
                name: "Validity",
                table: "Contracts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
