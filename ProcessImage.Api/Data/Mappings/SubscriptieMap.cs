using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProcessImage.Domain; // namespace-ul unde e Subscription

namespace ProcessImage.Data.Mappings
{
    public class SubscriptieMap : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscription");

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Tip)
                   .HasMaxLength(200);
            builder.Property(s => s.Pret)
                   .HasColumnType("decimal(18,4)")
                   .IsRequired();
            builder.Property(s => s.DimensiuneMaximaMb)
                   .IsRequired();
            builder.Property(s => s.SubscriptieProcesareID)
                   .IsRequired(false); 
            builder.HasIndex(s => s.Tip).HasDatabaseName("IX_Subscriptie_Tip");
            builder.HasIndex(s => s.Pret).HasDatabaseName("IX_Subscriptie_Pret");
            builder.HasIndex(s => s.SubscriptieProcesareID).HasDatabaseName("IX_Subscriptie_ProcesareID");
        }

    }
}
