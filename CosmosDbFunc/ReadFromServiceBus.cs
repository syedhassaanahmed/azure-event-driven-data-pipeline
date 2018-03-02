using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace CosmosDbFunc
{
    public static class ReadFromServiceBus
    {
        private static DocumentClient _documentClient;

        [FunctionName("ReadFromServiceBus")]
        public static async Task Run([ServiceBusTrigger("productsQueue", AccessRights.Listen, 
            Connection = "SERVICEBUS_CONNECTION")]string productJson, ILogger log)
        {
            if (_documentClient == null)
                _documentClient = CreateDocumentClient();

            var product = JsonConvert.DeserializeObject<Document>(productJson);
            await UpsertProductAsync(_documentClient, product, log);
        }

        private static async Task UpsertProductAsync(DocumentClient client, Document product, ILogger log)
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri("masterdata", "product");
            var response = await client.UpsertDocumentAsync(collectionLink, product);
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
