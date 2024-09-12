using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ProcesamientoMensajes.Extensiones
{
    public static class ExtensionServicioColas
    {
        public static IServiceCollection AgregarProcesadorColaServiceBus(this IServiceCollection servicios, IConfiguration config)
        {
            var opcionesProcesador = new ServiceBusProcessorOptions
            {
                MaxAutoLockRenewalDuration = TimeSpan.FromHours(config.GetValue<int>("Cola:DuracionRenovacionBloqueo"))
            };

            var clientOptions = new ServiceBusClientOptions
            {
                RetryOptions =
                {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 2,
                    Mode = ServiceBusRetryMode.Exponential
                }
            };

            var nombreCola = config.GetValue<string>("Cola:Nombre");
            var clienteServicioCola = new ServiceBusClient(config.GetValue<string>("Cola:ConnectionString"), clientOptions);

            var procesadorMensajes = clienteServicioCola.CreateProcessor(nombreCola, opcionesProcesador);
            var manejadorMensajes = clienteServicioCola.CreateSender(nombreCola, new ServiceBusSenderOptions());

            servicios.AddSingleton(procesadorMensajes);
            servicios.AddSingleton(manejadorMensajes);

            return servicios;
        }

        public static IServiceCollection AgregarProcesadorColaRabbitMQ(this IServiceCollection servicios, IConfiguration config)
        {
            // Configura ConnectionFactory
            servicios.AddSingleton(sp =>
            {
                var nombreCola = config.GetValue<string>("Cola:ConnectionString");
                var factory = new ConnectionFactory()
                {
                    HostName = nombreCola
                };
                return factory;
            });

            // Configura IConnection
            servicios.AddSingleton(sp =>
            {
                var factory = sp.GetRequiredService<ConnectionFactory>();
                return factory.CreateConnection();
            });

            return servicios;
        }

    }
}