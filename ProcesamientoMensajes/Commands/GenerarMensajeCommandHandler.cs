using MediatR;
using Serilog;

namespace ProcesamientoMensajes.Commands
{
    public class GenerarMensajeCommandHandle(ILogger logger) : IRequestHandler<GenerarMensajeCommand, Unit>
    {
        public Task<Unit> Handle(GenerarMensajeCommand request, CancellationToken cancellationToken)
        {
            logger.Information(request.Mensaje);

            return Task.FromResult(Unit.Value);
        }
    }
}
