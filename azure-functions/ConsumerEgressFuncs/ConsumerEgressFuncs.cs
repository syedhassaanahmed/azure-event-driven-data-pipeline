using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerEgressFuncs
{
    public class CosmosDbIdentity
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }
    }

    public class ConsumerData
    {
        public string ConsumerUrl { get; set; }
        public List<CosmosDbIdentity> ChangedProducts { get; set; }
    }

    public static class ConsumerEgressFuncs
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly DocumentClient DocumentClient = CreateDocumentClient();

        [FunctionName(nameof(OrchestrateConsumersFunc))]
        public static async Task OrchestrateConsumersFunc([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var changedProductIds = ctx.GetInput<List<CosmosDbIdentity>>();

            var retryOptions = new Microsoft.Azure.WebJobs.RetryOptions(firstRetryInterval: TimeSpan.FromSeconds(5),
                maxNumberOfAttempts: 3);

            var consumers = Environment.GetEnvironmentVariable("CONSUMERS", EnvironmentVariableTarget.Process)
                ?.Split('|');

            var parallelTasks = consumers.Select(x => CallSendToConsumerActivityAsync(ctx, retryOptions,
                new ConsumerData {ConsumerUrl = x, ChangedProducts = changedProductIds}));

            await Task.WhenAll(parallelTasks);
        }

        public static async Task CallSendToConsumerActivityAsync(DurableOrchestrationContext ctx,
            Microsoft.Azure.WebJobs.RetryOptions retryOptions, ConsumerData consumerData)
        {
            try
            {
                await ctx.CallActivityWithRetryAsync(nameof(SendToConsumerFunc), retryOptions, consumerData);
            }
            catch
            {
                //TODO: TEMPORARILY MARK THE CONSUMER AS BANNED IN CONSUMER_DB
            }
        }

        [FunctionName(nameof(SendToConsumerFunc))]
        public static async Task SendToConsumerFunc([ActivityTrigger] DurableActivityContext ctx)
        {
            var consumerData = ctx.GetInput<ConsumerData>();

            foreach (var product in consumerData.ChangedProducts)
            {
                var documentUri = UriFactory.CreateDocumentUri("masterdata", "product", product.Id);
                var document = await DocumentClient.ReadDocumentAsync(documentUri,
                    new RequestOptions { PartitionKey = new PartitionKey(product.PartitionKey) });

                var content = new StringContent(document.Resource.ToString(), Encoding.UTF8, "application/json");
                await HttpClient.PostAsync(consumerData.ConsumerUrl, content);
            }
        }

        private static DocumentClient CreateDocumentClient()
        {
            var endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT", EnvironmentVariableTarget.Process);
            var authKey = Environment.GetEnvironmentVariable("COSMOSDB_KEY", EnvironmentVariableTarget.Process);

            return new DocumentClient(new Uri(endpoint), authKey);
        }
    }
}