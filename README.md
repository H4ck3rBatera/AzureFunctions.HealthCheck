# AzureFunctions.HealthCheck

- Azure Functions
- .NET Core 3.1
- Microsoft.Extensions.Diagnostics.HealthChecks
- AspNetCore.HealthChecks.SqlServer
- Docker

### csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netcoreapp3.1</TargetFramework>
    <AzureFunctionsVersion>v3</AzureFunctionsVersion>
    <DockerFastModeProjectMountDirectory>/home/site/wwwroot</DockerFastModeProjectMountDirectory>
    <DockerDefaultTargetOS>Linux</DockerDefaultTargetOS>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AspNetCore.HealthChecks.SqlServer" Version="3.2.0" />
    <PackageReference Include="Microsoft.NET.Sdk.Functions" Version="3.0.11" />
    <PackageReference Include="Microsoft.VisualStudio.Azure.Containers.Tools.Targets" Version="1.10.13" />
  </ItemGroup>
  <ItemGroup>
    <None Update="host.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="local.settings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </None>
  </ItemGroup>
</Project>
```

### Controller
```csharp
public class HealthCheckFunction
    {
        private readonly HealthCheckService _healthCheck;
        public HealthCheckFunction(HealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        [FunctionName("Heartbeat")]
        public async Task<IActionResult> Heartbeat(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "heartbeat")] HttpRequest req,
           ILogger log, CancellationToken cancellationToken)
        {
            log.Log(LogLevel.Information, "Received heartbeat request");

            var healthReport = await _healthCheck.CheckHealthAsync(cancellationToken);

            string json = JsonConvert.SerializeObject(healthReport, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented,
                Converters = new Collection<JsonConverter>
                {
                     new StringEnumConverter()
                }
            });

            return new OkObjectResult(json);
        }
    }
```

### local.settings.json
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "ConnectionStringSqlServer": "Server=localhost,1433;Database=master;User ID=sa;Password=P@ssword",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  }
}
```

### Startup
```csharp
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
```

### docker-compose
```yaml
version: '3.4'

services:
    sql_server:
        image: mcr.microsoft.com/mssql/server:latest
        ports:
            - "1433:1433"
        environment:
            SA_PASSWORD: 'P@ssword'
            ACCEPT_EULA: 'Y'
```
