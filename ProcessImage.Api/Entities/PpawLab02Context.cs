using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProcessImage.Entities;

public partial class PpawLab02Context : DbContext
{
    public PpawLab02Context()
    {
    }

    public PpawLab02Context(DbContextOptions<PpawLab02Context> options)
        : base(options)
    {
    }

    public virtual DbSet<SubscripteProcesare> SubscripteProcesares { get; set; }

    public virtual DbSet<Subscriptie> Subscripties { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<TipProcesare> TipProcesares { get; set; }

    public virtual DbSet<Utilizator> Utilizators { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=PPAWLAB02;TrustServerCertificate=True;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=C5hkJCgp");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscripteProcesare>(entity =>
        {
            entity.ToTable("Subscripte_Procesare");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LimitaMax).HasColumnName("Limita_Max");

            entity.HasOne(d => d.Subscriptie).WithMany(p => p.SubscripteProcesares)
                .HasForeignKey(d => d.SubscriptieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subscripte_Procesare_Subscriptie");

            entity.HasOne(d => d.TipProcesare).WithMany(p => p.SubscripteProcesares)
                .HasForeignKey(d => d.TipProcesareId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subscripte_Procesare_Tip_Procesare");
        });

        modelBuilder.Entity<Subscriptie>(entity =>
        {
            entity.ToTable("Subscriptie");

            entity.HasIndex(e => e.Id, "IX_Subscriptio_ID_1");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.DimensiuneMaximaMb).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Pret).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Tip)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscription");

            entity.HasIndex(e => e.Pret, "IX_Subscriptie_Pret");

            entity.HasIndex(e => e.SubscriptieProcesareId, "IX_Subscriptie_ProcesareID");

            entity.HasIndex(e => e.Tip, "IX_Subscriptie_Tip");

            entity.Property(e => e.Pret).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SubscriptieProcesareId).HasColumnName("SubscriptieProcesareID");
            entity.Property(e => e.Tip).HasMaxLength(200);
        });

        modelBuilder.Entity<TipProcesare>(entity =>
        {
            entity.ToTable("Tip_Procesare");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Nume)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nume");
        });

        modelBuilder.Entity<Utilizator>(entity =>
        {
            entity.ToTable("Utilizator");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).IsUnicode(false);
            entity.Property(e => e.Nume)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Parola).IsUnicode(false);

            entity.HasOne(d => d.Subscriptie).WithMany(p => p.Utilizators)
                .HasForeignKey(d => d.SubscriptieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Utilizator_Subscriptie");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
