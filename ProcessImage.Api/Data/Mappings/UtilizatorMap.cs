using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProcessImage.Entities;

namespace ProcessImage.Data.Mappings
{
    public class UtilizatorMap
    {
        public void Configure(EntityTypeBuilder<Utilizator> builder)
        {
            builder.ToTable("Utilizator");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .IsRequired()
                   .ValueGeneratedOnAdd();

            builder.Property(u => u.Nume)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.Parola)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255);
            
            builder.Property(u => u.SubscriptieId)
                   .IsRequired();

            builder.HasOne(u => u.Subscriptie)
                   .WithMany(s => s.Utilizators)
                   .HasForeignKey(u => u.SubscriptieId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.Property(u => u.RolId)
                   .IsRequired();

            builder.HasOne(u => u.Rol)
                   .WithMany() 
                   .HasForeignKey(u => u.RolId)
                   .OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(u => u.Email)
                   .HasDatabaseName("IX_Utilizator_Email")
                   .IsUnique();

            builder.HasIndex(u => u.RolId)
                   .HasDatabaseName("IX_Utilizator_RolId");
        }

    }
}
