using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AzureFunctions.HealthCheck.Presentation;
using System;

[assembly: WebJobsStartup(typeof(Startup))]
namespace AzureFunctions.HealthCheck.Presentation
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddHealthChecks()
                .AddSqlServer(
                    name: "ConnectionStringSqlServer",
                    connectionString: Environment.GetEnvironmentVariable("ConnectionStringSqlServer"),
                    tags: new string[] { "sqlserver" });
        }
    }
}