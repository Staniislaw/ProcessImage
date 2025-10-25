using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProcessImage.Entities; // entitatea SubscripteProcesare

namespace ProcessImage.Data.Mappings
{
    public class SubscripteProcesareMap : IEntityTypeConfiguration<SubscripteProcesare>
    {
        public void Configure(EntityTypeBuilder<SubscripteProcesare> builder)
        {
            builder.ToTable("SubscripteProcesare");
            builder.HasKey(sp => sp.Id);
            builder.Property(sp => sp.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(sp => sp.SubscriptieId)
                   .IsRequired();
            builder.Property(sp => sp.TipProcesareId)
                   .IsRequired();
            builder.Property(sp => sp.LimitaMax)
                   .IsRequired(false);
            
            builder.HasIndex(sp => sp.SubscriptieId)
                   .HasDatabaseName("IX_SubscripteProcesare_SubscriptieId");

            builder.HasIndex(sp => sp.TipProcesareId)
                   .HasDatabaseName("IX_SubscripteProcesare_TipProcesareId");
        }
    }
}
