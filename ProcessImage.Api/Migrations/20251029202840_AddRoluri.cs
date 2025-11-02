using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class AddRoluri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RolId",
                table: "Utilizator",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NumeRol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Utilizator_RolId",
                table: "Utilizator",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_Id",
                table: "Rol",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Rol_NumeRol",
                table: "Rol",
                column: "NumeRol",
                unique: true,
                filter: "[NumeRol] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilizator_Rol_RolId",
                table: "Utilizator",
                column: "RolId",
                principalTable: "Rol",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilizator_Rol_RolId",
                table: "Utilizator");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropIndex(
                name: "IX_Utilizator_RolId",
                table: "Utilizator");

            migrationBuilder.DropColumn(
                name: "RolId",
                table: "Utilizator");
        }
    }
}
