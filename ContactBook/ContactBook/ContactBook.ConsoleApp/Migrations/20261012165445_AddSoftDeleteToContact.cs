using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactBook.ConsoleApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Contacts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Contacts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Contacts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Contacts");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
