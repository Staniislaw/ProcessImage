using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ProcessImage.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

var startups = new List<IBaseStartup>
{
    new DbStartup(),
    new DependencyStartup(),
    new MvcStartup(),
    new SwaggerStartup()
};
foreach (var startup in startups)
{
    startup.ConfigureServices(builder.Services, builder.Configuration);
}

var app = builder.Build();

foreach (var startup in startups)
{
    startup.Configure(app, app.Environment);
}

app.Run();
