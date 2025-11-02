using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ProcessImage.Entities;

public class RolMap : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("Rol");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
               .IsRequired()
               .ValueGeneratedOnAdd();
        builder.Property(r => r.NumeRol)
               .IsRequired(false)
               .HasMaxLength(50);
        builder.HasIndex(r => r.NumeRol)
               .HasDatabaseName("IX_Rol_NumeRol")
               .IsUnique();
        builder.HasIndex(r => r.Id)
               .HasDatabaseName("IX_Rol_Id");
    }
}
