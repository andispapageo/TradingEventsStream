#!/bin/bash

echo "Waiting for Kafka Connect to be ready..."
until curl -s http://localhost:8083/ > /dev/null; do
    echo "Kafka Connect not ready yet, waiting..."
    sleep 5
done

echo "Kafka Connect is ready!"

# Deploy Trading Events Sink
echo "Deploying Trading Events Sink Connector..."
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d @kafka-connect-configs/trading-events-sink.json

# Deploy Order Executions Sink
echo "Deploying Order Executions Sink Connector..."
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d @kafka-connect-configs/order-executions-sink.json

# Deploy Market Data Sink
echo "Deploying Market Data Sink Connector..."
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d @kafka-connect-configs/market-data-sink.json

echo ""
echo "Checking connector status..."
curl -s http://localhost:8083/connectors | jq

echo ""
echo "Connector deployment complete!"