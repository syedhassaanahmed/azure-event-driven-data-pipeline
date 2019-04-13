# azure-event-driven-data-pipeline
[![Build Status](https://dev.azure.com/syedhassaanahmed/azure-event-driven-data-pipeline/_apis/build/status/azure-event-driven-data-pipeline-CI)](https://dev.azure.com/syedhassaanahmed/azure-event-driven-data-pipeline/_build/latest?definitionId=9)

## Problem
A large retailer with many source systems, wants a single source of truth of their data and be able to send updates to their consumers whenever this data is changed. They want to support an unpredictable load, with a max spike of 1500 req/sec.

>[This blog post](https://medium.com/@hasssaaannn/building-single-source-of-truth-using-serverless-and-nosql-bca6c9d45eeb) describes the contents of this repo in detail.

## Architecture
<div style=""><img src="docs/images/architecture.png"/></center></div>

## Deployment
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://azuredeploy.net/)

The entire deployment can be orchestrated using [ARM template](https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-create-first-template) `azuredeploy.json`.

To deploy using [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest);
```
az group deployment create -g <RESOURCE_GROUP> --template-file azuredeploy.json
```

Once the deployment is complete, the only **manual step** is to copy `ConsumerReceiveFunc` URL from the Azure portal and paste it multiple times (pipe `|` delimited) in `ConsumerEgressFunc` -> App Settings -> `CONSUMERS`.

## Running load tests
We perform the load tests using [Azure Container Instances](https://docs.microsoft.com/en-us/azure/container-instances/container-instances-overview). After creating resources using the above ARM template, run the following load testing script;
```
./generate-load.sh <RESOURCE_GROUP> <CONTAINER_NAME> https://http-ingress-func.azurewebsites.net/api/HttpIngressFunc?code=<FUNCTION_KEY>
```

Here is how to stream logs from the container;
```
az container attach -g <RESOURCE_GROUP> -n <CONTAINER_NAME>
```

## Measuring Cosmos DB RUs using Application Insights
When we upsert into Cosmos DB, we log the [Request Units](https://docs.microsoft.com/en-us/azure/cosmos-db/request-units) consumed in [Application Insights](https://docs.microsoft.com/en-us/azure/application-insights/app-insights-overview). The following [Kusto](https://docs.microsoft.com/en-us/azure/kusto/query/) query renders a timechart of RUs consumed in the last 10 minutes, aggregated on 10 seconds.
```sql
customMetrics
| where timestamp > ago(10m)
    and name == "product_RU"
| summarize avg(value) by bin(timestamp, 10s)
| render timechart
```

## Resources
[Choose between Azure services that deliver messages](https://docs.microsoft.com/en-us/azure/event-grid/compare-messaging-services)

[Choose between Flow, Logic Apps, Functions, and WebJobs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-compare-logic-apps-ms-flow-webjobs)

[Durable Functions overview](https://docs.microsoft.com/en-us/azure/azure-functions/durable-functions-overview)

[Understanding Serverless Cold Start](https://blogs.msdn.microsoft.com/appserviceteam/2018/02/07/understanding-serverless-cold-start/)

[Azure Function Apps: Performance Considerations](https://blogs.msdn.microsoft.com/amitagarwal/2018/04/03/azure-function-apps-performance-considerations/)

[Processing 100,000 Events Per Second on Azure Functions](https://blogs.msdn.microsoft.com/appserviceteam/2017/09/19/processing-100000-events-per-second-on-azure-functions/)

[Choose the right data store](https://docs.microsoft.com/en-us/azure/architecture/guide/technology-choices/data-store-overview)

[Modeling document data for NoSQL databases](https://docs.microsoft.com/en-us/azure/cosmos-db/modeling-data)

[A fast, serverless, big data pipeline powered by a single Azure Function](https://azure.microsoft.com/en-us/blog/a-fast-serverless-big-data-pipeline-powered-by-a-single-azure-function/)

[Load testing with Azure Container Instances and wrk](https://blog.vjrantal.net/2017/08/10/load-testing-with-azure-container-instances-and-wrk/)
