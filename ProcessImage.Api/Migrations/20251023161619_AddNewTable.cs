using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptie",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pret = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DimensiuneMaximaMb = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubscriptieProcesareId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptie", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipProcesare",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipProcesare", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizator",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nume = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Parola = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptieId = table.Column<long>(type: "bigint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilizator_Subscriptie_SubscriptieId",
                        column: x => x.SubscriptieId,
                        principalTable: "Subscriptie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscripteProcesare",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptieId = table.Column<long>(type: "bigint", nullable: false),
                    TipProcesareId = table.Column<long>(type: "bigint", nullable: false),
                    LimitaMax = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscripteProcesare", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscripteProcesare_Subscriptie_SubscriptieId",
                        column: x => x.SubscriptieId,
                        principalTable: "Subscriptie",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscripteProcesare_TipProcesare_TipProcesareId",
                        column: x => x.TipProcesareId,
                        principalTable: "TipProcesare",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscripteProcesare_SubscriptieId",
                table: "SubscripteProcesare",
                column: "SubscriptieId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscripteProcesare_TipProcesareId",
                table: "SubscripteProcesare",
                column: "TipProcesareId");

            migrationBuilder.CreateIndex(
                name: "IX_TipProcesare_Nume",
                table: "TipProcesare",
                column: "Nume");

            migrationBuilder.CreateIndex(
                name: "IX_Utilizator_SubscriptieId",
                table: "Utilizator",
                column: "SubscriptieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscripteProcesare");

            migrationBuilder.DropTable(
                name: "Utilizator");

            migrationBuilder.DropTable(
                name: "TipProcesare");

            migrationBuilder.DropTable(
                name: "Subscriptie");
        }
    }
}
