using IoTConsumer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoTConsumer
{
    public class MqttConsumerHostedService : IHostedService
    {
        private readonly MqttConsumerService _mqttConsumerService;
        private readonly ILogger<MqttConsumerHostedService> _logger;

        public MqttConsumerHostedService(
            MqttConsumerService mqttConsumerService,
            ILogger<MqttConsumerHostedService> logger)
        {
            _mqttConsumerService = mqttConsumerService;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting MQTT Consumer Service");
            await _mqttConsumerService.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping MQTT Consumer Service");
            await _mqttConsumerService.StopAsync();
        }
    }
}