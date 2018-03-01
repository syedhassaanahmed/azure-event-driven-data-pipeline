#r "Microsoft.Azure.Documents.Client"

using System;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

private static DocumentClient _documentClient;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ILogger log)
{
    if (_documentClient == null)
        _documentClient = CreateDocumentClient();

    dynamic product = await req.Content.ReadAsAsync<object>();
    await UpsertProductAsync(_documentClient, product, log);
}

private static async Task UpsertProductAsync(DocumentClient client, dynamic product, ILogger log)
{
    var collectionLink = UriFactory.CreateDocumentCollectionUri("masterdata", "product");
    var response = await client.UpsertDocumentAsync(collectionLink, product);
    log.LogMetric("product_RU", response.RequestCharge);
}

private static DocumentClient CreateDocumentClient()
{
    var endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT", EnvironmentVariableTarget.Process);
    var authKey = Environment.GetEnvironmentVariable("COSMOSDB_KEY", EnvironmentVariableTarget.Process);

    return new DocumentClient(new Uri(endpoint), authKey, ConsistencyLevel.ConsistentPrefix);
}