using MediatR;

namespace ProcesamientoMensajes.Commands
{
    public class GenerarMensajeCommandHandle : IRequestHandler<GenerarMensajeCommand, Unit>
    {
        public Task<Unit> Handle(GenerarMensajeCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine(request.Mensaje);

            return Task.FromResult(Unit.Value);
        }
    }
}
