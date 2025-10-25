using Data.SDK.Repository;

using ProcessImage.Domain;

using static System.Net.Mime.MediaTypeNames;
using ProcessImage.repository;
using ProcessImage.repository.Interface;
using ProcessImage.Repository;
using ProcessImage.Repository.Interfaces;
using ProcessImage.Services;
using ProcessImage.Services.Interface;
namespace ProcessImage.Infrastructure
{
    public class DependencyStartup : IBaseStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            //services.AddScoped<IAccesor, Accesor>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            services.AddScoped<IUtilizatorRepository, UtilizatorRepository>();
            services.AddScoped<IBaseService, BaseService>();
            //services.AddScoped<IUserService, UserService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }

}
