using Data.SDK.Repository;

using ProcessImage.Jobs;
using ProcessImage.Repository;
using ProcessImage.Repository.Interfaces;
using ProcessImage.Services;
using ProcessImage.Services.Interface;

using Quartz;
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
            services.AddScoped<IImageProcessingService,ImageProcessingService>();
            services.AddScoped<ILogCleanupService, LogCleanupService>();
            services.AddScoped<IDashBoardService, DashBoardService>();
            services.AddScoped<ISubscriptionsService, SubscriptionsService>();
            services.AddScoped<IUtilizatorService, UtilizatorService>();
            services.AddSingleton<ICacheService, CacheService>();

            services.AddMemoryCache();
            //.WithCronSchedule("0 */1 * * * ?")
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey("deleteLogsJob");

                q.AddJob<DeleteLogsJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("deleteLogsTrigger").WithCronSchedule("0 0 * * * ?")
                );
            });
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            //services.AddScoped<IUserService, UserService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }

}
