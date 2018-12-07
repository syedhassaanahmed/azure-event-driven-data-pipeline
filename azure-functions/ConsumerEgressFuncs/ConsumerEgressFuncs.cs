using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerEgressFuncs
{
    public static class ConsumerEgressFuncs
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [FunctionName(nameof(OrchestrateConsumersFunc))]
        public static async Task OrchestrateConsumersFunc([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var changedProducts = ctx.GetInput<IEnumerable<string>>();

            var retryOptions = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(5),
                maxNumberOfAttempts: 3);

            var consumers = Environment.GetEnvironmentVariable("CONSUMERS", EnvironmentVariableTarget.Process)
                ?.Split('|');

            var parallelTasks = consumers.Select(x => CallSendToConsumerActivityAsync(ctx, retryOptions, x, changedProducts));

            await Task.WhenAll(parallelTasks);
        }

        public static async Task CallSendToConsumerActivityAsync(DurableOrchestrationContext ctx,
            RetryOptions retryOptions, string consumerUrl, IEnumerable<string> changedProducts)
        {
            try
            {
                await ctx.CallActivityWithRetryAsync(nameof(SendToConsumerFunc), retryOptions, (consumerUrl, changedProducts));
            }
            catch
            {
                //TODO: TEMPORARILY MARK THE CONSUMER AS BANNED IN CONSUMER_DB
            }
        }

        [FunctionName(nameof(SendToConsumerFunc))]
        public static async Task SendToConsumerFunc([ActivityTrigger] (string consumerUrl, IEnumerable<string> changedProducts) consumerData, 
            ILogger log)
        {
            foreach (var product in consumerData.changedProducts)
            {
                var content = new StringContent(product, Encoding.UTF8, "application/json");
                await HttpClient.PostAsync(consumerData.consumerUrl, content);
            }
        }
    }
}