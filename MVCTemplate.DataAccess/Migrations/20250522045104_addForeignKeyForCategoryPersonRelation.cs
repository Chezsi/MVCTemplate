using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MVCTemplate.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addForeignKeyForCategoryPersonRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Persons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Categorys",
                columns: new[] { "IdCategory", "CodeCategory", "CreatedAt", "NameCategory", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "11C", new DateTime(2025, 5, 22, 12, 51, 3, 279, DateTimeKind.Local).AddTicks(4357), "C11", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, "22C", new DateTime(2025, 5, 22, 12, 51, 3, 279, DateTimeKind.Local).AddTicks(4371), "C22", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, "33C", new DateTime(2025, 5, 22, 12, 51, 3, 279, DateTimeKind.Local).AddTicks(4372), "C23", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CategoryId", "CreatedAt" },
                values: new object[] { 1, new DateTime(2025, 5, 22, 12, 51, 3, 279, DateTimeKind.Local).AddTicks(4500) });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CategoryId", "CreatedAt" },
                values: new object[] { 1, new DateTime(2025, 5, 22, 12, 51, 3, 279, DateTimeKind.Local).AddTicks(4502) });

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "CreatedAt" },
                values: new object[] { 1, new DateTime(2025, 5, 22, 12, 51, 3, 279, DateTimeKind.Local).AddTicks(4503) });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_CategoryId",
                table: "Persons",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons",
                column: "CategoryId",
                principalTable: "Categorys",
                principalColumn: "IdCategory",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Categorys_CategoryId",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Persons_CategoryId",
                table: "Persons");

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
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Persons");

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 11, 13, 59, 134, DateTimeKind.Local).AddTicks(3622));

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 11, 13, 59, 134, DateTimeKind.Local).AddTicks(3633));

            migrationBuilder.UpdateData(
                table: "Persons",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 22, 11, 13, 59, 134, DateTimeKind.Local).AddTicks(3634));
        }
    }
}
