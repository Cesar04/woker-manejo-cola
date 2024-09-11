using Microsoft.Extensions.Hosting;
using ProcesamientoMensajes.Procesadores;

namespace ProcesamientoMensajes
{
    public class Worker(ProcesadorMensajeRabbitMQ procesadorMensaje) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            procesadorMensaje.Iniciar();

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
