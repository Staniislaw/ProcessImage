using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class AddNewSubscriptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tip = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Pret = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DimensiuneMaximaMb = table.Column<int>(type: "int", nullable: false),
                    SubscriptieProcesareID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptie_Pret",
                table: "Subscription",
                column: "Pret");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptie_ProcesareID",
                table: "Subscription",
                column: "SubscriptieProcesareID");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptie_Tip",
                table: "Subscription",
                column: "Tip");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscription");
        }
    }
}
