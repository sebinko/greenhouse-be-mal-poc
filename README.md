# SEP4 Backend and MAL Proof of Concept

This project demonstrates a system architecture with three main components that communicate with each other:

- **mal-model**: ML component that trains and builds a classification model
- **mal-api**: REST API that serves the trained model for predictions
- **IoT Simulator**: Simulates sensor data and publishes via MQTT
- **IoT Consumer**: C# service that consumes MQTT messages and stores them in the database
- **Data**: C# database connection module for data persistence

## System Architecture

- The components communicate through:
  - **Database**: Shared storage for model data and predictions
  - **MQTT**: Message broker for IoT communication
  - **REST API**: HTTP endpoints for predictions

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

### Stop the System

```sh
docker compose -f docker-compose-local.yml down
```

## Docker-Only Deployment

Note that this system is designed to run in Docker containers only. The connection strings for the database, MQTT broker, and other services rely on Docker container naming and networking. Running components individually outside Docker will require configuration changes.

## Testing the Communication

### API Health Check

```sh
curl http://localhost:5050/health
```

### Test Prediction API

```sh
curl -X POST http://localhost:5050/predict \
  -H "Content-Type: application/json" \
  -d '{"cap-shape": "x", "cap-surface": "s", "cap-color": "n", "does-bruise-or-bleed": "t", "gill-attachment": "f", "gill-spacing": "c", "gill-color": "k", "stem-height": 2.0, "stem-width": 0.5, "stem-surface": "s", "stem-color": "w", "has-ring": "t", "ring-type": "p", "habitat": "u", "season": "s"}'
```

## Communication Flow

1. The ML model trains and stores model artifacts
2. The API loads the model and exposes endpoints
3. The IoT simulator publishes sensor data via MQTT
4. The IoT Consumer (C#) receives MQTT messages
5. Data is stored in the database via the Data connection module
6. Data flows through the system for predictions and analysis

The mushroom classification is just an example domain - the focus is on the component communication patterns and system architecture.
