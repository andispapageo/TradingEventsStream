# TradingEventsStream

A .NET 8 Web API for streaming trading events and market data to Kafka. This service enables order submission, market data publishing, and Kafka health monitoring for trading platforms.

---

## Features

- Submit trade orders via REST, published to Kafka
- Publish real-time market data to Kafka
- Health check endpoint for API and Kafka broker connectivity
- List Kafka topics and partition information

---

## Prerequisites

- .NET 8 SDK
- Kafka broker (default: `localhost:9092`)
- Docker (optional, for containerized deployment)

---

## Configuration

Kafka connection and producer options are set in `appsettings.json` under the `Kafka` section.  
The default bootstrap server is `localhost:9092`.  
You can adjust producer settings such as acknowledgments, idempotence, and timeouts as needed.

---

## Running the API

**To run locally:**
1. Build the project using the .NET CLI:
dotnet build
2. Run the API project:
dotnet run --project TradingEventsStream

**To run with Docker:**
1. Build the Docker image:
docker run -p 5000:80 -e "Kafka__BootstrapServers=host.docker.internal:9092" trading-events-stream


---

## API Endpoints

### 1. Submit Order

- **POST** `/api/trading/orders`
- Request body:  
- `symbol`, `orderType`, `side`, `quantity`, `price`, `userId`
- Response:  
- `orderId`, `status`, `message`, `timestamp`

### 2. Publish Market Data

- **POST** `/api/trading/market-data`
- Request body: 
- `symbol`, `bidPrice`, `askPrice`, `lastPrice`, `volume`, `bidSize`, `askSize`
- Response:  
- Confirmation message

### 3. Health Check

- **GET** `/api/trading/health`
- Response: 
- API and Kafka connection status, timestamp, broker details

### 4. List Kafka Topics

- **GET** `/api/trading/kafka/topics`
- Response: 
- List of Kafka topics, partition counts, and any topic errors

---

## Development

- Logging is configured in `appsettings.json`.
- Ensure Kafka is running and accessible for local development.
- Follow project and contribution guidelines as defined in `CONTRIBUTING.md` if present.

---

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

---

