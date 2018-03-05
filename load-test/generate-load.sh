#!/bin/bash

if [ -z "$1" ]; then echo "RESOURCE_GROUP was not supplied"; exit 1; fi && RESOURCE_GROUP=$1
if [ -z "$2" ]; then echo "CONTAINER_NAME was not supplied"; exit 1; fi && CONTAINER_NAME=$2
if [ -z "$3" ]; then echo "TARGET_URL was not supplied"; exit 1; fi && TARGET_URL=$3

az container delete -g $RESOURCE_GROUP --name $CONTAINER_NAME -y

# recreate Cosmos DB collection with partition key
DB_NAME=masterdata
COLLECTION_NAME=product
COSMOS_DB=$(az cosmosdb list -g $RESOURCE_GROUP --query "[0].name" -o tsv)
az cosmosdb database create -g $RESOURCE_GROUP -n $COSMOS_DB -d $DB_NAME
az cosmosdb collection delete -g $RESOURCE_GROUP -n $COSMOS_DB -d $DB_NAME -c $COLLECTION_NAME
az cosmosdb collection delete -g $RESOURCE_GROUP -n $COSMOS_DB -d $DB_NAME -c leases
az cosmosdb collection create -g $RESOURCE_GROUP -n $COSMOS_DB -d $DB_NAME -c $COLLECTION_NAME \
    --throughput 25000 --partition-key-path "/productGroupId" --indexing-policy @indexing-policy.json

SCRIPT_URL=https://raw.githubusercontent.com/syedhassaanahmed/azure-event-driven-data-pipeline/master/load-test/products.lua
WRK_OPTIONS="-t1 -c100 -d2m -R1500 --latency"
WRK_IMAGE=syedhassaanahmed/wrk2-with-online-script

az container create -g $RESOURCE_GROUP --name $CONTAINER_NAME --image $WRK_IMAGE \
    -l westeurope --restart-policy Never -e \
    SCRIPT_URL="$SCRIPT_URL" \
    TARGET_URL="$TARGET_URL" \
    WRK_OPTIONS="$WRK_OPTIONS" \
    WRK_HEADER="content-type: application/json"