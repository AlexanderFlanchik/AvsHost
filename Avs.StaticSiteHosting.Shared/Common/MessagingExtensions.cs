using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Avs.StaticSiteHosting.Shared.Common;

public static class MessagingExtensions
{   
    /// <summary>
    /// Adds RabbitMq messaging with default settings.
    /// </summary>
    /// <param name="services">Services collection</param>
    /// <param name="configuration">IConfiguration instance</param>
    /// <param name="optionsConfigure">Additional configuration of MassTransit options</param>
    /// <returns>Service collection instance</returns>
    public static IServiceCollection AddMessaging(this IServiceCollection services, 
        IConfiguration configuration, 
        Action<IBusRegistrationConfigurator>? optionsConfigure = null)
    {
        var rabbitMqSettings = new RabbitMqSettings();
        configuration.GetSection("RabbitMqSettings").Bind(rabbitMqSettings);

        services.AddMassTransit(options =>
        {
            options.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration.GetConnectionString("AvsBroker");
                Console.WriteLine($"RabbitMQ Host: {rabbitMqHost}");
                cfg.Host(rabbitMqHost);

                cfg.ConfigureEndpoints(context);
            });

            if (optionsConfigure is not null)
            {
                optionsConfigure(options);
            }
        });

        return services;
    }
}