using System.Reflection; // Trebuie adăugat pentru a folosi Assembly

using Base.SDK;

using Microsoft.EntityFrameworkCore;

namespace Data.SDK
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<BaseEntity>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.StartsWith("ProcessImage") || assembly.FullName.StartsWith("Data.SDK"))
                {
                    modelBuilder.ApplyConfigurationsFromAssembly(assembly);
                }
            }
        }

    }
}