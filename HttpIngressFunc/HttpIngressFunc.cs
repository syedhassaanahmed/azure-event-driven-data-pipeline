using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;

namespace HttpIngressFunc
{
    public static class HttpIngressFunc
    {
        [FunctionName(nameof(HttpIngressFunc))]
        [return: ServiceBus("productsQueue", AccessRights.Send, Connection = "SERVICEBUS_CONNECTION")]
        public static string Run([HttpTrigger] dynamic product, TraceWriter log)
        {
            return product.ToString();
        }
    }
}
