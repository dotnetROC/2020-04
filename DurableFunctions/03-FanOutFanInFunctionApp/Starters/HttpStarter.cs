using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FanOutFanInFunctionApp.Starters
{
    public static class HttpStarter
	{
        [FunctionName("FanOut_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
                [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
                [DurableClient] IDurableOrchestrationClient starter,
                ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("FanOutOrchestator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
