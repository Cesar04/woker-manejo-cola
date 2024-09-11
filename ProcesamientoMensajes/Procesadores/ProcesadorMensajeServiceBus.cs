using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ProcesamientoMensajes.Dtos;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace ProcesamientoMensajes.Procesadores
{
    public class ProcesadorMensajeServiceBus
    {
        private readonly ServiceBusProcessor _procesadorMensajesCola;
        private readonly Dictionary<string, Type> _tiposComando;
        private readonly IServiceProvider _proveedorServicios;
        private readonly Serilog.ILogger _logger;

        public ProcesadorMensajeServiceBus(ServiceBusProcessor procesadorMensajesCola, Serilog.ILogger logger, IServiceProvider proveedorServicios)
        {
            _procesadorMensajesCola = procesadorMensajesCola;
            _proveedorServicios = proveedorServicios;
            _logger = logger;

            var ensambladoDeAplicacion = Assembly.GetExecutingAssembly();
            _tiposComando = ensambladoDeAplicacion.GetTypes()
               .Where(tipo => tipo.GetInterfaces()
               .ToList()
               .Exists(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>)))
               .ToDictionary(prop => prop.Name, prop => prop);
        }

        public async Task Iniciar()
        {
            _procesadorMensajesCola.ProcessMessageAsync += ManejarMensajeAsync;
            _procesadorMensajesCola.ProcessErrorAsync += ManejarErroresAsync;

            await _procesadorMensajesCola.StartProcessingAsync();
        }

        public async Task Detener()
        {
            await _procesadorMensajesCola.StopProcessingAsync();
        }

        private Task ManejarMensajeAsync(ProcessMessageEventArgs argumentos)
        {
            var mensajeString = Encoding.Default.GetString(argumentos.Message.Body);

            _logger.Information(mensajeString);

            var objetoMensaje = JsonSerializer.Deserialize<MensajeWorker<object>>(mensajeString)!;

            if (!_tiposComando.TryGetValue(objetoMensaje.TipoComando.ToString(), out var tipoComando))
            {
                throw new ArgumentException($"No se encontró un tipo de comando para {objetoMensaje.TipoComando}");
            }

            return ProcesarMensajeAsync(argumentos, objetoMensaje.Contenido.ToString()!, tipoComando);
        }

        private async Task ProcesarMensajeAsync(ProcessMessageEventArgs argumentos, string cuerpoMensaje, Type tipoMensaje)
        {
            using var alcance = _proveedorServicios.CreateScope();
            var mediador = alcance.ServiceProvider.GetRequiredService<IMediator>();
            var comando = JsonSerializer.Deserialize(cuerpoMensaje, tipoMensaje)!;

            await mediador.Send(comando);
            await argumentos.CompleteMessageAsync(argumentos.Message);
        }

        private Task ManejarErroresAsync(ProcessErrorEventArgs args)
        {
            ArgumentNullException.ThrowIfNull(args);

            string mensajeError = $"El controlador de mensajes encontró una excepción: {args.Exception.Message}.";
            string mensajeSeguimiento = $"Contexto de excepción para la resolución de problemas: " +
                $"- Endpoint: {args.FullyQualifiedNamespace} - Ruta de Entidad: {args.EntityPath} - Acción Ejecutada: {args.Identifier}";

            _logger.Error(exception: args.Exception, messageTemplate: mensajeError);
            _logger.Information(mensajeSeguimiento);

            return Task.CompletedTask;
        }
    }
}
