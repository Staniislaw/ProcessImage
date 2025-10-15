namespace ProcessImage.Infrastructure
{
    public interface IBaseStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);
        void Configure(IApplicationBuilder app, IWebHostEnvironment env);
    }

}
