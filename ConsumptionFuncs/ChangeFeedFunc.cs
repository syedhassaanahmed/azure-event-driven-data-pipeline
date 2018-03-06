using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace ConsumptionFuncs
{
    public static class ChangeFeedFunc
    {
        [FunctionName(nameof(ChangeFeedFunc))]
        public static Task Run([CosmosDBTrigger(
            databaseName: "masterdata",
            collectionName: "product",
            CreateLeaseCollectionIfNotExists = true,
            ConnectionStringSetting = "COSMOSDB_CONNECTION",
            LeaseCollectionName = "leases")]JArray input, [OrchestrationClient] DurableOrchestrationClient starter, TraceWriter log)
        {
            if (input != null && input.Count > 0)
            {
                // Send Doc Ids to the orchestrator, otherwise a big Document might exceed 64 KB Storage Queue limit
                var products = input.Select(x => new CosmosDbIdentity
                {
                    Id = x.Value<string>("id"),
                    PartitionKey = x.Value<string>("productGroupId")
                });

                return starter.StartNewAsync(nameof(DurableConsumerFuncs.OrchestrateConsumersFunc), products);
            }

            return Task.CompletedTask;
        }
    }
}
