using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactBook.ConsoleApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Contacts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Contacts");
        }
    }
}
