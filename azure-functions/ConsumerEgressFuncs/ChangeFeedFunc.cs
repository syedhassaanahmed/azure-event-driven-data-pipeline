using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ConsumerEgressFuncs
{
    public static class ChangeFeedFunc
    {
        [FunctionName(nameof(ChangeFeedFunc))]
        public static Task Run([CosmosDBTrigger(
                databaseName: "masterdata",
                collectionName: "product",
                ConnectionStringSetting = "COSMOSDB_CONNECTION",
                LeaseCollectionName = "leases", 
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> input, 
            [OrchestrationClient] DurableOrchestrationClient starter, 
            ILogger log)
        {
            if (input == null || input.Count <= 0)
                return Task.CompletedTask;

            var products = input.Select(x => x.ToString());
            return starter.StartNewAsync(nameof(ConsumerEgressFuncs.OrchestrateConsumersFunc), products);
        }
    }
}
