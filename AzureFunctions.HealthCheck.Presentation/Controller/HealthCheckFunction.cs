using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureFunctions.HealthCheck.Presentation.Controller
{
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

            var json = JsonSerializer.Serialize(healthReport,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                    });

            return new OkObjectResult(json);
        }
    }
}