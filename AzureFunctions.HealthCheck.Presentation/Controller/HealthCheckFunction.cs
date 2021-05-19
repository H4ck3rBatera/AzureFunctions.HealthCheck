using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.ObjectModel;

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
}