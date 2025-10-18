using Data.SDK.Repository;

using ProcessImage.Domain;

using static System.Net.Mime.MediaTypeNames;
using ProcessImage.repository;
namespace ProcessImage.Infrastructure
{
    public class DependencyStartup : IBaseStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAccesor, Accesor>();
            //services.AddScoped<IUserService, UserService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }

}
