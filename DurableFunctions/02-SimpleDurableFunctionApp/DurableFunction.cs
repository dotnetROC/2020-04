using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SimpleDurableFunctionApp
{
    public static class DurableFunction
    {
        #region *** Starter functions ***

        // *** Starter function with an HTTP Trigger ***
        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string requestBody = await req.Content.ReadAsStringAsync().ConfigureAwait(false);
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name ?? "Unknown";

            string instanceId = await starter.StartNewAsync<string>("Function1", name);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // *** Starter function with a Queue Trigger ***
        [FunctionName("Function1_QueueStart")]
        public static async Task QueueStart(
            [QueueTrigger("durable-function-trigger", Connection = "QueueConnectionString")] string input,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the queue message
            dynamic data = JsonConvert.DeserializeObject(input);
            string name = data?.name ?? "Unknown";

            string instanceId = await starter.StartNewAsync<string>("Function1", name);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
        }

        #endregion

        // *** Orchestrator Function ***
        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            string name = context.GetInput<string>();

            //log = context.CreateReplaySafeLogger(log);
            if (!context.IsReplaying)
            {
                log.LogInformation($"Executing Orchestrator with an input of '{name}'");
            }

            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", $"{name} from Tokyo"));
            //context.SetCustomStatus("Tokyo");
            
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", $"{name} from Seattle"));
            //context.SetCustomStatus("Tokyo");
            
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", $"{name} from London"));
            //context.SetCustomStatus("Tokyo");

            // returns ["Hello x from Tokyo!", "Hello x from Seattle!", "Hello x from London!"]
            log.LogInformation($"Output: { string.Join(", ", outputs ) }");
            return outputs;
        }

        // *** Activity Function ***
        [FunctionName("Function1_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
    }
}