using Microsoft.Extensions.Configuration;
namespace Data.SDK.Repository
{
    public class Repository<T> : BaseRepository<T, ApplicationDbContext> where T : class
    {
        public Repository(ApplicationDbContext applicationDbContext, IConfiguration configuration)
        : base(applicationDbContext, configuration)
        {
        }

    }
}
