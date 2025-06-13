using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addPersonsToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.DeleteData(
                table: "Categorys",
                keyColumn: "IdCategory",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorys",
                keyColumn: "IdCategory",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorys",
                keyColumn: "IdCategory",
                keyValue: 3);*/

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Persons",
                columns: new[] { "Id", "Name", "Position" },
                values: new object[,]
                {
                    { 1, "Name1", "E1" },
                    { 2, "Name2", "E2" },
                    { 3, "Name3", "E3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Persons");

            /*
            migrationBuilder.InsertData(
                table: "Categorys",
                columns: new[] { "IdCategory", "CodeCategory", "CreatedAt", "NameCategory", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "1C", new DateTime(2025, 5, 19, 17, 25, 0, 3, DateTimeKind.Local).AddTicks(4402), "C1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "2C", new DateTime(2025, 5, 19, 17, 25, 0, 3, DateTimeKind.Local).AddTicks(4412), "C2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "3C", new DateTime(2025, 5, 19, 17, 25, 0, 3, DateTimeKind.Local).AddTicks(4413), "C3", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });*/
        }
    }
}
