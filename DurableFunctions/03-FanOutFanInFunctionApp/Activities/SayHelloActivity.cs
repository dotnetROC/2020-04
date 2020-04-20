using System.Threading;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace FanOutFanInFunctionApp.Activities
{
    public static class SayHelloActivity
    {
        [FunctionName("FanOutSayHelloActivity")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            Thread.Sleep(5000);

            log.LogInformation($"Saying hello to {name}.");

            return $"Hello {name}!";
        }
    }
}
