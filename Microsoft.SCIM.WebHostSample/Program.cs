using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SCIM;
using Microsoft.SCIM.WebHostSample.Provider;
using System;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);
var monitoringBehavior = new ConsoleMonitor();
var providerBehavior = new InMemoryProvider();
var configuration = builder.Configuration;

// get aspnet core environment variable 
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Environment: {environment}");

// Add services to the container.
// This replaces services configured in Startup.ConfigureServices.
builder.Services.AddControllers();
ConfigureServices(builder.Services);

// Build the app.
var app = builder.Build();

if (environment == "Development")
{
    app.UseDeveloperExceptionPage();
}

app.UseHsts();
app.UseRouting();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(
    (IEndpointRouteBuilder endpoints) =>
    {
        endpoints.MapDefaultControllerRoute();
    });

app.Run();


void ConfigureServices(IServiceCollection services)
{
    void ConfigureMvcNewtonsoftJsonOptions(MvcNewtonsoftJsonOptions options) => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

    void ConfigureAuthenticationOptions(AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }

    void ConfigureJwtBearerOptons(JwtBearerOptions options)
    {
        if (environment == "Development")
        {
            options.TokenValidationParameters =
               new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = false,
                   ValidateIssuerSigningKey = false,
                   ValidIssuer = configuration["Token:TokenIssuer"],
                   ValidAudience = configuration["Token:TokenAudience"],
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:IssuerSigningKey"]))
               };
        }
        else
        {
            options.Authority = configuration["Token:TokenIssuer"];
            options.Audience = configuration["Token:TokenAudience"];
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = AuthenticationFailed
            };
        }

    }

    services.AddAuthentication(ConfigureAuthenticationOptions).AddJwtBearer(ConfigureJwtBearerOptons);
    services.AddControllers().AddNewtonsoftJson(ConfigureMvcNewtonsoftJsonOptions);

    services.AddSingleton(typeof(IProvider), providerBehavior);
    services.AddSingleton(typeof(IMonitor), monitoringBehavior);
}

Task AuthenticationFailed(AuthenticationFailedContext arg)
{
    // For debugging purposes only!
    string authenticationExceptionMessage = $"{{AuthenticationFailed: '{arg.Exception.Message}'}}";

    arg.Response.ContentLength = authenticationExceptionMessage.Length;
    arg.Response.Body.WriteAsync(
        Encoding.UTF8.GetBytes(authenticationExceptionMessage),
        0,
        authenticationExceptionMessage.Length);

    return Task.FromException(arg.Exception);
}