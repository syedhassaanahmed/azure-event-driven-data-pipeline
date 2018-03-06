using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsumptionFuncs
{
    public class ConsumerData
    {
        public string ConsumerId { get; set; }
        public List<Document> ChangedProducts { get; set; }
    }

    public static class DurableConsumerFuncs
    {
        private static readonly Dictionary<string, string> Consumers = new Dictionary<string, string>
        {
            {"C1", "http://"},
            {"C2", "http://"},
            {"C3", "http://"}
        };

        [FunctionName(nameof(ConsumerOrchestrator))]
        public static async Task ConsumerOrchestrator([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var changedProducts = ctx.GetInput<List<Document>>();

            var parallelTasks = Consumers.Keys.Select(x => ctx.CallActivityAsync(nameof(SendToConsumerAsync), 
                new ConsumerData { ConsumerId = x, ChangedProducts = changedProducts }));

            await Task.WhenAll(parallelTasks);
        }

        [FunctionName(nameof(SendToConsumerAsync))]
        public static async Task SendToConsumerAsync([ActivityTrigger] DurableActivityContext ctx)
        {
            var consumerData = ctx.GetInput<ConsumerData>();
            var url = Consumers[consumerData.ConsumerId];

            var policy = Policy.Handle<Exception>().WaitAndRetry(3,
                attempt => TimeSpan.FromSeconds(0.1 * Math.Pow(2, attempt)));

            try
            {
                await policy.ExecuteAsync(async () =>
                {
                    using (var httpClient = new HttpClient())
                    {
                        foreach (var changedProduct in consumerData.ChangedProducts)
                        {
                            await httpClient.PostAsync(url, new StringContent(changedProduct.ToString()));
                        }
                    }
                });
            }
            catch
            {
                // CRITICAL ERROR: Ban the consumer temporarily!!!
                throw;
            }
        }
    }
}
