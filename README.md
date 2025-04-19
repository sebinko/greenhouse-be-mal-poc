# SEP4 Backend and MAL Proof of Concept

This project demonstrates a system architecture with three main components that communicate with each other:

- **mal-model**: ML component that trains and builds a classification model
- **mal-api**: REST API that serves the trained model for predictions
- **API**: Main entry point for frontend communication
- **IoT Simulator**: Simulates sensor data and publishes via MQTT
- **IoT Consumer**: C# service that consumes MQTT messages and stores them in the database
- **Data**: C# database connection module for data persistence

## Docker-Only Deployment

Note that this system is designed to run in Docker containers only. The connection strings for the database, MQTT broker, and other services rely on Docker container naming and networking. Running components individually outside Docker will require configuration changes.

## System Architecture

- The components communicate through:
  - **Database**: Shared storage for model data and predictions
  - **MQTT**: Message broker for IoT communication
  - **REST API**: HTTP endpoints for predictions and frontend integration

## Important Note

**This is only a proof of concept** and was quickly made to demonstrate communication between components. The final project architecture will differ significantly. This implementation is simplified to show that the core communication patterns work.

## Quick Start

### Start the System

```sh
docker compose -f docker-compose-local.yml up -d --build
```

### Train the Model

```sh
docker compose -f docker-compose-local.yml --profile build-model up mal-model
```

### API Usage

The API is the main entry point for frontend communication. You can access the API endpoints at `http://localhost:8080/api`.

#### Mushroom Classification Example

```sh
curl -X 'POST' \
  'http://localhost:8080/api/Model/predict' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{"cap-shape": "x", "cap-surface": "s", "cap-color": "n", "does-bruise-or-bleed": "t", "gill-attachment": "f", "gill-spacing": "c", "gill-color": "k", "stem-height": 2.0, "stem-width": 0.5, "stem-surface": "s", "stem-color": "w", "has-ring": "t", "ring-type": "p", "habitat": "u", "season": "s"}'
```

#### Health Check

You can check the health status of the API and its upstream services:

```sh
curl -X 'GET' 'http://localhost:8080/api/Model/health'
```

### Stop the System

```sh
docker compose -f docker-compose-local.yml down
```


## Communication Flow

1. The ML model trains and stores model artifacts
2. The mal-api loads the model and exposes prediction endpoints
3. The main API provides a unified interface for frontend communication
4. The IoT simulator publishes sensor data via MQTT
5. The IoT Consumer (C#) receives MQTT messages
6. Data is stored in the database via the Data connection module
7. Data flows through the system for predictions and analysis

The mushroom classification is just an example domain - the focus is on the component communication patterns and system architecture.

## API Documentation

API documentation is available via Swagger UI when the system is running:
```sh
http://localhost:8080/swagger/index.html
```
