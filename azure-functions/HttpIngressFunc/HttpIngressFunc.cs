using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public static class HttpIngressFunc
    {
        [FunctionName(nameof(HttpIngressFunc))]
        [return: ServiceBus("productsQueue", Connection = "SERVICEBUS_CONNECTION")]
        public static string Run([HttpTrigger(AuthorizationLevel.Function, "post")]
            dynamic product, ILogger log)
        {
            return product.ToString();
        }
    }
}
