using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace HumanInteractionFunctionApp
{
    public static class CallbackFunction
    {
        // *** Starter ***
        [FunctionName("CallbackFunction_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("CallbackFunction", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        // *** Orchestrator ***
        [FunctionName("CallbackFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync("RequestApproval", null);

            using (var timeoutCts = new CancellationTokenSource())
            {
                // 1 minute timeout for demo purposes
                DateTime dueTime = context.CurrentUtcDateTime.AddMinutes(1);
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                Task<bool> approvalEvent = context.WaitForExternalEvent<bool>("ApprovalEvent");
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();
                    await context.CallActivityAsync("ProcessApproval", approvalEvent.Result);
                }
                else
                {
                    await context.CallActivityAsync("ProcessEscalated", null);
                }
            }
        }

        // *** Activities ***
        [FunctionName("RequestApproval")]
        public static Task SendApprovalRequest([ActivityTrigger] string name, ILogger log)
        {
            log.LogWarning("Approval request sent!");
            return Task.CompletedTask;
        }

        [FunctionName("ProcessApproval")]
        public static string Approved([ActivityTrigger] string name, ILogger log)
        {
            log.LogWarning($"Process Approved!");
            return $"Approved!";
        }

        [FunctionName("ProcessEscalated")]
        public static string Escalate([ActivityTrigger] string name, ILogger log)
        {
            log.LogWarning("Process Escalated!");
            return "Escalated!";
        }
    }
}