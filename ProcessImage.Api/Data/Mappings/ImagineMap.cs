using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProcessImage.Entities;

namespace ProcessImage.Entities.Maps
{
    public class ImagineMap : IEntityTypeConfiguration<Imagine>
    {
        public void Configure(EntityTypeBuilder<Imagine> builder)
        {
            builder.ToTable("Imagini");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id)
                   .ValueGeneratedOnAdd();
            builder.Property(i => i.Nume)
                   .IsRequired()
                   .HasMaxLength(200);
            builder.Property(i => i.Tip)
                   .IsRequired()
                   .HasMaxLength(50);
            builder.Property(i => i.UtilizatorId)
                   .IsRequired();
            builder.Property(i => i.DataIncarcarii)
                   .IsRequired();
            builder.Property(i => i.CaleFisier)
                   .IsRequired()
                   .HasMaxLength(500);
            builder.HasIndex(i => i.UtilizatorId)
                   .HasDatabaseName("IX_Imagini_UtilizatorId");
            builder.HasIndex(i => i.Nume)
                   .HasDatabaseName("IX_Imagini_Nume");
        }
    }
}
