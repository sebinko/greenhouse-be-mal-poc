using Data.DbContext;
using IoTConsumer.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace IoTConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            // Run database migrations
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<GreenhouseDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    logger.LogInformation("Applying database migrations...");
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Database migrations applied successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying migrations");
                    return;
                }
            }

            Console.WriteLine("Greenhouse MQTT Consumer started. Press Ctrl+C to exit.");
            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Register DbContext
                    services.AddDbContext<GreenhouseDbContext>(options => 
                    {
                        // Connection string is configured in OnConfiguring method of the context
                    }, ServiceLifetime.Transient);
                    
                    // Register MqttConsumerService
                    services.AddSingleton<MqttConsumerService>();
                    
                    // Register hosted service to start the MQTT consumer
                    services.AddHostedService<MqttConsumerHostedService>();
                });
    }
}