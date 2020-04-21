using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MonitorFunctionApp
{
    public static class MonitorFunction
    {
        private static Random rnd = new Random();

        // *** Starter ***
        [FunctionName("MonitorOrchestrator_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("MonitorOrchestrator", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // *** Orchestrator ***
        [FunctionName("MonitorOrchestrator")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            int pollingInterval = 3;
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(2);

            while (context.CurrentUtcDateTime < expiryTime)
            {
                var randomNumber = await context.CallActivityAsync<int>("GetRandomNumber", 0);
                if (randomNumber == 7)
                {
                    // Perform an action when a condition is met.
                    log.LogInformation($"Random number search successful ({randomNumber})!");
                    break;
                }

                // number not found, try again!
                if (!context.IsReplaying)
                {
                    log.LogWarning($"Random number search not successful ({randomNumber})");
                }

                // Orchestration sleeps until this time.
                var nextCheck = context.CurrentUtcDateTime.AddSeconds(pollingInterval);
                await context.CreateTimer(nextCheck, CancellationToken.None);
            }

            // Perform more work here, or let the orchestration end.
        }

        // *** Activity ***
        [FunctionName("GetRandomNumber")]
        public static int RandomNumber([ActivityTrigger] int number, ILogger log)
        {
            int num = rnd.Next(1, 10);

            log.LogWarning($"Random number inside Activity ({num})");
            
            return num;
        }
    }
}