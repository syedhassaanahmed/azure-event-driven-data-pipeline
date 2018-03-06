using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;
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
                return starter.StartNewAsync(nameof(DurableConsumerFuncs.OrchestrateConsumersAsync), input);
            }

            return Task.CompletedTask;
        }
    }
}
