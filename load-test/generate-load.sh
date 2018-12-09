#!/bin/bash

if [ -z "$1" ]; then echo "RESOURCE_GROUP was not supplied"; exit 1; fi && RESOURCE_GROUP=$1
if [ -z "$2" ]; then echo "CONTAINER_NAME was not supplied"; exit 1; fi && CONTAINER_NAME=$2
if [ -z "$3" ]; then echo "TARGET_URL was not supplied"; exit 1; fi && TARGET_URL=$3

CONTAINER_EXISTS=$(az container list -g $RESOURCE_GROUP --query "[].contains(name, '$CONTAINER_NAME')" -o tsv)
if [[ "$CONTAINER_EXISTS" = true ]]; then
    az container stop -g $RESOURCE_GROUP -n $CONTAINER_NAME
fi

DATABASE=masterdata
COLLECTION=product
LEASES=leases
COSMOS_DB=$(az cosmosdb list -g $RESOURCE_GROUP --query "[0].name" -o tsv)

DATABASE_EXISTS=$(az cosmosdb database exists -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -o tsv)
if [[ "$DATABASE_EXISTS" != true ]]; then
  az cosmosdb database create -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE
fi

# Recreate Cosmos DB leases and products collection with partition key
LEASES_EXISTS=$(az cosmosdb collection exists -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -c $LEASES -o tsv)
if [[ "$LEASES_EXISTS" = true ]]; then
  az cosmosdb collection delete -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -c $LEASES
fi
az cosmosdb collection create -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -c $LEASES

COLLECTION_EXISTS=$(az cosmosdb collection exists -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -c $COLLECTION -o tsv)
if [[ "$COLLECTION_EXISTS" = true ]]; then
  az cosmosdb collection delete -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -c $COLLECTION
fi
az cosmosdb collection create -g $RESOURCE_GROUP -n $COSMOS_DB -d $DATABASE -c $COLLECTION \
    --throughput 25000 --partition-key-path "/partitionKey" --indexing-policy @indexing-policy.json

SCRIPT_URL=https://raw.githubusercontent.com/syedhassaanahmed/azure-event-driven-data-pipeline/master/load-test/products.lua
WRK_OPTIONS="-t1 -c100 -d2m -R1500 --latency"
WRK_IMAGE=syedhassaanahmed/wrk2-with-online-script

# Timestamp environment variable is used to trigger an ACI update, otherwise container will remain stopped
az container create -g $RESOURCE_GROUP -n $CONTAINER_NAME --image $WRK_IMAGE \
    -l westeurope --restart-policy Never -e \
    SCRIPT_URL="$SCRIPT_URL" \
    TARGET_URL="$TARGET_URL" \
    WRK_OPTIONS="$WRK_OPTIONS" \
    WRK_HEADER="content-type: application/json" \
    TIMESTAMP="$(date +%s)"