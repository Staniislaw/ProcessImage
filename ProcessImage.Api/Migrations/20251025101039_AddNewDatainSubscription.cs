using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    public partial class AddNewDatainSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO TipProcesare (Nume) 
                VALUES 
                    ('Resize'),
                    ('Crop'),
                    ('Filter'),
                    ('Compress'),
                    ('Rotate'),
                    ('Watermark');
            ");
            migrationBuilder.Sql(@"
                INSERT INTO Subscriptie (Tip, Pret, DimensiuneMaximaMb, SubscriptieProcesareId) 
                VALUES 
                    ('Free', 0, 5, 1),
                    ('Premium', 9.99, 50, 2),
                    ('Professional', 29.99, 200, 3);
            ");

            migrationBuilder.Sql(@"
                INSERT INTO SubscripteProcesare (SubscriptieId, TipProcesareId, LimitaMax)
                VALUES
                    (1, 1, 10),  -- Free: Resize max 10
                    (1, 2, 5);   -- Free: Crop max 5
            ");

            migrationBuilder.Sql(@"
                INSERT INTO SubscripteProcesare (SubscriptieId, TipProcesareId, LimitaMax)
                VALUES
                    (2, 1, 100),  -- Premium: Resize max 100
                    (2, 2, 100),  -- Premium: Crop max 100
                    (2, 3, 50),   -- Premium: Filter max 50
                    (2, 4, 50),   -- Premium: Compress max 50
                    (2, 5, 50);   -- Premium: Rotate max 50
            ");

            migrationBuilder.Sql(@"
                INSERT INTO SubscripteProcesare (SubscriptieId, TipProcesareId, LimitaMax)
                VALUES
                    (3, 1, NULL),  -- Professional: Resize unlimited
                    (3, 2, NULL),  -- Professional: Crop unlimited
                    (3, 3, NULL),  -- Professional: Filter unlimited
                    (3, 4, NULL),  -- Professional: Compress unlimited
                    (3, 5, NULL),  -- Professional: Rotate unlimited
                    (3, 6, NULL);  -- Professional: Watermark unlimited
            ");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM SubscripteProcesare;");
            migrationBuilder.Sql("DELETE FROM Subscriptie;");
            migrationBuilder.Sql("DELETE FROM TipProcesare;");
        }
    }
}
