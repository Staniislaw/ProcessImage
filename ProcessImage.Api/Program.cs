using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ProcessImage.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Adaugă cheia în configuration
builder.Configuration["Jwt:Key"] = "my_super_secret_key_that_is_long_enough_for_jwt_256_bits_minimum!";

var key = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

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