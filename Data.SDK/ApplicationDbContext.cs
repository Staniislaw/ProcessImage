using System.Linq.Expressions;
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

            //FILTRU PENTRU EXLCUDEREA isActive.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var isActiveProperty = entityType.ClrType.GetProperty("isActive",
                    BindingFlags.Public | BindingFlags.Instance);

                if (isActiveProperty != null && isActiveProperty.PropertyType == typeof(bool))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var propertyAccess = Expression.Property(parameter, isActiveProperty);
                    var lambda = Expression.Lambda(propertyAccess, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

    }
}