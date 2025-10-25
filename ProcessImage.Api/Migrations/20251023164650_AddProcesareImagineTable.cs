using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class AddProcesareImagineTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProceseImagini",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImagineId = table.Column<long>(type: "bigint", nullable: false),
                    TipProcesareId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataProcesare = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProceseImagini", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProceseImagini_Imagini_ImagineId",
                        column: x => x.ImagineId,
                        principalTable: "Imagini",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProceseImagini_TipProcesare_TipProcesareId",
                        column: x => x.TipProcesareId,
                        principalTable: "TipProcesare",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProceseImagini_ImagineId",
                table: "ProceseImagini",
                column: "ImagineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProceseImagini_TipProcesareId",
                table: "ProceseImagini",
                column: "TipProcesareId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProceseImagini");
        }
    }
}
