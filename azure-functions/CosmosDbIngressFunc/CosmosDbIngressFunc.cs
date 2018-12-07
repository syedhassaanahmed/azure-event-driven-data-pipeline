using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CosmosDbIngressFunc
{
    public static class CosmosDbIngressFunc
    {
        private static readonly DocumentClient DocumentClient = CreateDocumentClient();

        [FunctionName(nameof(CosmosDbIngressFunc))]
        public static async Task Run([ServiceBusTrigger("productsQueue", Connection = "SERVICEBUS_CONNECTION")]
            string productJson, ILogger log)
        {
            var product = JsonConvert.DeserializeObject<Document>(productJson);
            await UpsertProductAsync(product, log);
        }

        private static async Task UpsertProductAsync(Document product, ILogger log)
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri("masterdata", "product");

            // Explicitly adding partitionKey to the data, gives us the flexibility of modifying it later
            product.SetPropertyValue("partitionKey", product.GetPropertyValue<string>("productGroupId"));

            var response = await DocumentClient.UpsertDocumentAsync(collectionLink, product);
            log.LogMetric("product_RU", response.RequestCharge);
        }

        private static DocumentClient CreateDocumentClient()
        {
            var endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT", EnvironmentVariableTarget.Process);
            var authKey = Environment.GetEnvironmentVariable("COSMOSDB_KEY", EnvironmentVariableTarget.Process);

            return new DocumentClient(new Uri(endpoint), authKey, null, ConsistencyLevel.ConsistentPrefix);
        }
    }
}
