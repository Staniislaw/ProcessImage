using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class AddImagineTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LimitaMax",
                table: "SubscripteProcesare",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Imagini",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UtilizatorId = table.Column<long>(type: "bigint", nullable: false),
                    DataIncarcarii = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CaleFisier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imagini", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Imagini_Utilizator_UtilizatorId",
                        column: x => x.UtilizatorId,
                        principalTable: "Utilizator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Imagini_Nume",
                table: "Imagini",
                column: "Nume");

            migrationBuilder.CreateIndex(
                name: "IX_Imagini_UtilizatorId",
                table: "Imagini",
                column: "UtilizatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Imagini");

            migrationBuilder.AlterColumn<long>(
                name: "LimitaMax",
                table: "SubscripteProcesare",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
