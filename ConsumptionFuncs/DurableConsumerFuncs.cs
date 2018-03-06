using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsumptionFuncs
{
    public class ConsumerData
    {
        public string ConsumerUrl { get; set; }
        public List<Document> ChangedProducts { get; set; }
    }

    public static class DurableConsumerFuncs
    {
        [FunctionName(nameof(OrchestrateConsumersAsync))]
        public static async Task OrchestrateConsumersAsync([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var changedProducts = ctx.GetInput<List<Document>>();

            var retryOptions = new RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(5),
                maxNumberOfAttempts: 3);

            var consumers = Environment.GetEnvironmentVariable("CONSUMERS", EnvironmentVariableTarget.Process)
                .Split(new[] { '|' });

            var parallelTasks = consumers.Select(x => CallSendToConsumerActivityAsync(ctx, retryOptions,
                new ConsumerData { ConsumerUrl = x, ChangedProducts = changedProducts }));

            await Task.WhenAll(parallelTasks);
        }

        public static async Task CallSendToConsumerActivityAsync(DurableOrchestrationContext ctx, RetryOptions retryOptions, 
            ConsumerData consumerData)
        {
            try
            {
                await ctx.CallActivityWithRetryAsync(nameof(SendToConsumerAsync), retryOptions, consumerData);
            }
            catch
            {
                //TODO: TEMPORARILY MARK THE CONSUMER AS BANNED IN CONSUMERDB
            }
        }

        [FunctionName(nameof(SendToConsumerAsync))]
        public static async Task SendToConsumerAsync([ActivityTrigger] DurableActivityContext ctx)
        {
            var consumerData = ctx.GetInput<ConsumerData>();

            using (var httpClient = new HttpClient())
            {
                foreach (var changedProduct in consumerData.ChangedProducts)
                {
                    await httpClient.PostAsync(consumerData.ConsumerUrl, new StringContent(changedProduct.ToString()));
                }
            }
        }
    }
}
