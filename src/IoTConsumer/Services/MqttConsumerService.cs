using Data.DbContext;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IoTConsumer.Services
{
    public class MqttConsumerService
    {
        // MQTT Broker settings
        private const string MqttBroker = "mosquitto";
        private const int MqttPort = 1883;
        private const string ClientId = "greenhouse-consumer";

        // Topics
        private const string SensorTopic = "greenhouse/sensors";
        private const string WateringTopic = "greenhouse/control/watering";
        private const string LightTopic = "greenhouse/control/light";
        private const string FertilizerTopic = "greenhouse/control/fertilizer";

        // Latest data
        private SensorReading _latestSensorReading = new();
        private ControlState _latestControlState = new();

        private readonly GreenhouseDbContext _dbContext;
        private readonly ILogger<MqttConsumerService> _logger;
        private IMqttClient _mqttClient = null!; // Using null! to suppress warning

        public MqttConsumerService(GreenhouseDbContext dbContext, ILogger<MqttConsumerService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Greenhouse MQTT Consumer");
            _logger.LogInformation("------------------------");
            _logger.LogInformation($"Connecting to MQTT broker at {MqttBroker}:{MqttPort}");

            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            // Configure client options
            var options = new MqttClientOptionsBuilder()
                .WithClientId(ClientId)
                .WithTcpServer(MqttBroker, MqttPort)
                .WithCleanSession()
                .Build();

            // Set up handlers
            _mqttClient.ApplicationMessageReceivedAsync += HandleMessageReceived;

            try
            {
                // Connect to the broker
                await _mqttClient.ConnectAsync(options, cancellationToken);
                _logger.LogInformation("Connected to MQTT broker");

                // Subscribe to sensor topic with correct method
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(SensorTopic)
                    .Build());
                _logger.LogInformation($"Subscribed to {SensorTopic}");

                // Subscribe to control topics with correct method
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(WateringTopic)
                    .Build());
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(LightTopic)
                    .Build());
                await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic(FertilizerTopic)
                    .Build());
                _logger.LogInformation("Subscribed to control topics");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to MQTT broker");
                throw;
            }
        }

        public async Task StopAsync()
        {
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
                _logger.LogInformation("Disconnected from MQTT broker");
            }
        }

        private async Task HandleMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            string topic = args.ApplicationMessage.Topic;
            string payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            _logger.LogInformation($"Message received on topic: {topic}");

            try
            {
                if (topic == SensorTopic)
                {
                    await ProcessSensorMessage(payload);
                }
                else if (topic.StartsWith("greenhouse/control/"))
                {
                    await ProcessControlMessage(topic, payload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
            }
        }

        private async Task ProcessSensorMessage(string payload)
        {
            var sensorData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payload);

            if (sensorData != null)
            {
                _latestSensorReading = new SensorReading
                {
                    AirTemperature = sensorData.ContainsKey("air_temperature") ? sensorData["air_temperature"].GetDouble() : 0,
                    AirHumidity = sensorData.ContainsKey("air_humidity") ? sensorData["air_humidity"].GetDouble() : 0,
                    SoilHumidity = sensorData.ContainsKey("soil_humidity") ? sensorData["soil_humidity"].GetDouble() : 0,
                    LightLevel = sensorData.ContainsKey("light_level") ? sensorData["light_level"].GetInt32() : 0
                };

                // Save to database
                await _dbContext.SensorReadings.AddAsync(_latestSensorReading);
                await _dbContext.SaveChangesAsync();

                DisplaySensorReading(_latestSensorReading);
            }
        }

        private async Task ProcessControlMessage(string topic, string payload)
        {
            bool isOn = payload.Trim().ToUpper() == "ON";

            // Create a new control state object
            var controlState = new ControlState
            {
                Watering = _latestControlState.Watering,
                Light = _latestControlState.Light,
                Fertilizer = _latestControlState.Fertilizer
            };

            // Update the control state based on topic
            switch (topic)
            {
                case WateringTopic:
                    controlState.Watering = isOn;
                    break;
                case LightTopic:
                    controlState.Light = isOn;
                    break;
                case FertilizerTopic:
                    controlState.Fertilizer = isOn;
                    break;
            }

            // Update the latest state and save to database
            _latestControlState = controlState;
            await _dbContext.ControlStates.AddAsync(controlState);
            await _dbContext.SaveChangesAsync();

            DisplayControlState(_latestControlState);
        }

        private void DisplaySensorReading(SensorReading reading)
        {
            _logger.LogInformation("SENSOR READINGS:");
            _logger.LogInformation($"  Air Temperature: {reading.AirTemperature:F1} °C");
            _logger.LogInformation($"  Air Humidity: {reading.AirHumidity:F1} %");
            _logger.LogInformation($"  Soil Humidity: {reading.SoilHumidity:F1} %");
            _logger.LogInformation($"  Light Level: {reading.LightLevel} lux");
            _logger.LogInformation($"  Timestamp: {reading.Timestamp}");
        }

        private void DisplayControlState(ControlState state)
        {
            _logger.LogInformation("CONTROL STATES:");
            _logger.LogInformation($"  Watering: {(state.Watering ? "ON" : "OFF")}");
            _logger.LogInformation($"  Light: {(state.Light ? "ON" : "OFF")}");
            _logger.LogInformation($"  Fertilizer: {(state.Fertilizer ? "ON" : "OFF")}");
            _logger.LogInformation($"  Timestamp: {state.Timestamp}");
        }
    }
}