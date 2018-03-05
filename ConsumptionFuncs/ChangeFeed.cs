using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace ConsumptionFuncs
{
    public static class ChangeFeed
    {
        [FunctionName("ChangeFeed")]
        public static void Run([CosmosDBTrigger(
            databaseName: "masterdata",
            collectionName: "product",
            CreateLeaseCollectionIfNotExists = true,
            ConnectionStringSetting = "COSMOSDB_CONNECTION",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input, TraceWriter log)
        {
            if (input != null && input.Count > 0)
            {
                log.Verbose("Documents modified " + input.Count);
                log.Verbose("First document Id " + input[0].Id);
            }
        }
    }
}
