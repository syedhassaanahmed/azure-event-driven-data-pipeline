using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ConsumerReceiveFunc
{
    public static class ConsumerReceiveFunc
    {
        [FunctionName(nameof(ConsumerReceiveFunc))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
            ILogger log)
        {
            var product = await req.ReadAsStringAsync();

            log.LogInformation(product);

            return new Random().Next(0, 10) < 6
                ? (ActionResult)new OkObjectResult("Product received")
                : new BadRequestObjectResult("Random error");
        }
    }
}
