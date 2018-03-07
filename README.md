# azure-event-driven-data-pipeline
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

Content of this repo is demonstrated during Stockholm Azure meetup https://www.meetup.com/Stockholm-Azure-Meetup/events/247951748/

## Running load tests
We perform the load tests using [Azure Container Instances](https://docs.microsoft.com/en-us/azure/container-instances/container-instances-overview). After creating resources using the above ARM template, run the following load testing script;
```
./generate-load.sh azure-meetup loadgen-container https://http-ingress-func.azurewebsites.net/api/HttpIngressFunc?code=<FUNCTION_KEY>
```

Here is how to stream logs from the container;
```
az container attach -g azure-meetup -n loadgen-container
```

## Measuring RUs in Application Insights
When we upsert into Cosmos DB, we log the [Request Units](https://docs.microsoft.com/en-us/azure/cosmos-db/request-units) consumed in [Application Insights](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-overview). The following Application Insights analytics query renders a timechart of RUs consumed, aggregated on 10 seconds.
```sql
customMetrics
| where timestamp > datetime("2018-03-05T12:26:00")
    and name == "product_RU"
| summarize avg(value) by name, bin(timestamp, 10s)
| render timechart
```

## Resources
[Choose between Azure services that deliver messages](https://docs.microsoft.com/en-us/azure/event-grid/compare-messaging-services)

[Choose between Flow, Logic Apps, Functions, and WebJobs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-compare-logic-apps-ms-flow-webjobs)

[Durable Functions overview](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview)

[Azure Functions quickstart starter sample](https://github.com/Azure-Samples/functions-quickstart)

[Understanding Serverless Cold Start](https://blogs.msdn.microsoft.com/appserviceteam/2018/02/07/understanding-serverless-cold-start/)

[Choose the right data store](https://docs.microsoft.com/en-us/azure/architecture/guide/technology-choices/data-store-overview)

[Modeling document data for NoSQL databases](https://docs.microsoft.com/en-us/azure/cosmos-db/modeling-data)
