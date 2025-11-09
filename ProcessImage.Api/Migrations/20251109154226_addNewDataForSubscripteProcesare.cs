using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProcessImage.Migrations
{
    /// <inheritdoc />
    public partial class addNewDataForSubscripteProcesare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DECLARE @SubFree INT = (SELECT Id FROM Subscriptie WHERE Tip = 'Free');
                DECLARE @SubPremium INT = (SELECT Id FROM Subscriptie WHERE Tip = 'Premium');
                DECLARE @SubProfessional INT = (SELECT Id FROM Subscriptie WHERE Tip = 'Professional');

                DECLARE @TipCompress INT = (SELECT Id FROM TipProcesare WHERE Nume = 'Compress');
                DECLARE @TipCrop INT = (SELECT Id FROM TipProcesare WHERE Nume = 'Crop');
                DECLARE @TipFilter INT = (SELECT Id FROM TipProcesare WHERE Nume = 'Filter');
                DECLARE @TipResize INT = (SELECT Id FROM TipProcesare WHERE Nume = 'Resize');
                DECLARE @TipRotate INT = (SELECT Id FROM TipProcesare WHERE Nume = 'Rotate');
                DECLARE @TipTransferColors INT = (SELECT Id FROM TipProcesare WHERE Nume = 'TransferColors');
                DECLARE @TipWatermark INT = (SELECT Id FROM TipProcesare WHERE Nume = 'Watermark');                

                INSERT INTO SubscripteProcesare (SubscriptieId, TipProcesareId, LimitaMax)
                VALUES
                    (@SubFree, @TipCompress, 10),
                    (@SubFree, @TipCrop, 5),
                    (@SubFree, @TipFilter, 5),
                    (@SubFree, @TipResize, 5),
                    (@SubFree, @TipRotate, 10),
                    (@SubFree, @TipTransferColors, 3),
                    (@SubFree, @TipWatermark, 10),

                    (@SubPremium, @TipCompress, 15),
                    (@SubPremium, @TipCrop, 10),
                    (@SubPremium, @TipFilter, 10),
                    (@SubPremium, @TipResize, 10),
                    (@SubPremium, @TipRotate, 10),
                    (@SubPremium, @TipTransferColors, 9),
                    (@SubPremium, @TipWatermark, 20),

                    (@SubProfessional, @TipCompress, 120),
                    (@SubProfessional, @TipCrop, NULL),
                    (@SubProfessional, @TipFilter, 90),
                    (@SubProfessional, @TipResize, 90),
                    (@SubProfessional, @TipRotate, 90),
                    (@SubProfessional, @TipTransferColors, NULL),
                    (@SubProfessional, @TipWatermark, 100);
            ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE spl
                FROM SubscriptieProcesareLimite spl
                WHERE spl.SubscriptieId IN (
                    SELECT Id FROM Subscriptie WHERE Tip IN ('Free','Premium','Professional')
                )
                AND spl.TipProcesareId IN (
                    SELECT Id FROM TipProcesare WHERE Nume IN (
                        'ProcesareTip1','ProcesareTip2','ProcesareTip3','ProcesareTip4','ProcesareTip5','ProcesareTip6'
                    )
                );
            ");
        }

    }
}
