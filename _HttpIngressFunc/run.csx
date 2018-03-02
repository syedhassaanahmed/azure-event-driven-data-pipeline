using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log, IAsyncCollector<string> outputSbMsgTopic)
{
    dynamic product = await req.Content.ReadAsAsync<object>();   
    await outputSbMsgTopic.AddAsync(product.ToString());
    return req.CreateResponse(HttpStatusCode.OK);
}
