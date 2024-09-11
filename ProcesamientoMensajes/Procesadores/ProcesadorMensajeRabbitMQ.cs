using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcesamientoMensajes.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ProcesamientoMensajes.Procesadores
{
    public class ProcesadorMensajeRabbitMQ
    {
        private readonly IModel _channel;
        private readonly Dictionary<string, Type> _tiposComando;
        private readonly IServiceProvider _proveedorServicios;
        private readonly ILogger _logger;
        private readonly string _queueName;

        public ProcesadorMensajeRabbitMQ(IConnection connection,
            ILogger logger, IServiceProvider proveedorServicios, IConfiguration configuration)
        {
            _channel = connection.CreateModel();
            _proveedorServicios = proveedorServicios;
            _logger = logger;

            _queueName = configuration.GetValue<string>("Cola:Nombre") ?? string.Empty;

            var ensambladoDeAplicacion = Assembly.GetExecutingAssembly();
            _tiposComando = ensambladoDeAplicacion.GetTypes()
               .Where(tipo => tipo.GetInterfaces()
               .ToList()
               .Exists(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
               .ToDictionary(prop => prop.Name, prop => prop);
        }

        public void Iniciar()
        {
            _channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) => await ManejarMensajeAsync(model, ea);

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
        }

        public void Detener()
        {
            _channel.Close();
        }

        private async Task ManejarMensajeAsync(object? model, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body.ToArray();
                var mensajeString = Encoding.UTF8.GetString(body);

                _logger.Information(mensajeString);

                var objetoMensaje = JsonSerializer.Deserialize<MensajeWorker<object>>(mensajeString)!;

                if (!_tiposComando.TryGetValue(objetoMensaje.TipoComando.ToString(), out var tipoComando))
                {
                    throw new ArgumentException($"No se encontró un tipo de comando para {objetoMensaje.TipoComando}");
                }

                await ProcesarMensajeAsync(objetoMensaje.Contenido.ToString()!, tipoComando);
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                await ManejarErroresAsync(ex, ea);
            }
        }

        private async Task ProcesarMensajeAsync(string cuerpoMensaje, Type tipoMensaje)
        {
            using var alcance = _proveedorServicios.CreateScope();
            var mediador = alcance.ServiceProvider.GetRequiredService<IMediator>();
            var comando = JsonSerializer.Deserialize(cuerpoMensaje, tipoMensaje)!;

            await mediador.Send(comando);
        }

        private Task ManejarErroresAsync(Exception ex, BasicDeliverEventArgs ea)
        {
            ArgumentNullException.ThrowIfNull(ex);

            string mensajeError = $"El controlador de mensajes encontró una excepción: {ex.Message}.";
            string mensajeSeguimiento = $"Contexto de excepción para la resolución de problemas: " +
                $"- Ruta de Entidad: {_queueName} - Acción Ejecutada: {ea.RoutingKey}";

            _logger.Error(exception: ex, messageTemplate: mensajeError);
            _logger.Information(mensajeSeguimiento);

            // Opcional: Puedes volver a poner el mensaje en la cola o moverlo a una cola de mensajes muertos (DLQ)
            _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);

            return Task.CompletedTask;
        }
    }
}
