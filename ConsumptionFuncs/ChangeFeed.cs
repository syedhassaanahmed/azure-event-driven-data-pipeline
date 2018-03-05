using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json.Linq;

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
            LeaseCollectionName = "leases")]JArray input, TraceWriter log)
        {
            if (input != null && input.Count > 0)
            {
                log.Verbose("Documents modified " + input.Count);
                var firstDocument = input[0].ToObject<Document>();
                log.Verbose("First document Id " + firstDocument.Id);
            }
        }
    }
}
