using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FanOutFanInFunctionApp.Orchestrators
{
    public static class FanOutInOrchestrator
    {
        [FunctionName("FanOutOrchestator")]
        public static async Task<string> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            var tasks = new List<Task<string>>();

            tasks.Add(context.CallActivityAsync<string>("FanOutSayHelloActivity", "Tokyo"));
            tasks.Add(context.CallActivityAsync<string>("FanOutSayHelloActivity", "Seattle"));
            tasks.Add(context.CallActivityAsync<string>("FanOutSayHelloActivity", "London"));

            await Task.WhenAll(tasks);

            var builder = new StringBuilder();
            foreach (var task in tasks)
            {
                builder.AppendFormat("{0}, ", task.Result);
            }

            // returns "Hello Tokyo!, Hello Seattle!, Hello London!"
            var output = builder.ToString();

            log.LogInformation($"Output {output.Substring(0, output.Length - 1)}");

            return output.Substring(0, output.Length - 1);
        }
    }
}