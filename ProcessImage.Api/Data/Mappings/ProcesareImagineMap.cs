
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ProcessImage.Entities.Maps
{
    public class ProcesareImagineMap : IEntityTypeConfiguration<ProcesareImagine>
    {
        public void Configure(EntityTypeBuilder<ProcesareImagine> builder)
        {
            builder.ToTable("ProceseImagini");
            builder.HasKey(pi => pi.Id);
            builder.Property(pi => pi.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(pi => pi.ImagineId)
                   .IsRequired();
            builder.Property(pi => pi.TipProcesareId)
                   .IsRequired();
            builder.Property(pi => pi.Status)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(pi => pi.DataProcesare)
                   .IsRequired();
            builder.HasIndex(pi => pi.ImagineId)
                   .HasDatabaseName("IX_ProceseImagini_ImagineId");
            builder.HasIndex(pi => pi.TipProcesareId)
                   .HasDatabaseName("IX_ProceseImagini_TipProcesareId");
        }
    }
}
