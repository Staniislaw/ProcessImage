using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProcessImage.Domain;
using ProcessImage.Entities;
namespace ProcessImage.Data.Mappings
{
    public class TipProcesareMap : IEntityTypeConfiguration<TipProcesare>
    {
        public void Configure(EntityTypeBuilder<TipProcesare> builder)
        {
            builder.ToTable("TipProcesare");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Nume)
                .HasMaxLength(200)
                .IsRequired();
            builder.HasIndex(t => t.Nume)
                .HasDatabaseName("IX_TipProcesare_Nume");
            builder.HasMany(t => t.SubscripteProcesares)
                .WithOne(s => s.TipProcesare) 
                .HasForeignKey(s => s.TipProcesareId) 
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
