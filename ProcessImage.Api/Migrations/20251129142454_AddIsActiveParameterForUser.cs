using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveParameterForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Utilizator",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Utilizator");
        }
    }
}
