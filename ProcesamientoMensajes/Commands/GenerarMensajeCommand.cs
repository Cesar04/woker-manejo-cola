using MediatR;

namespace ProcesamientoMensajes.Commands
{
    public record GenerarMensajeCommand(string Mensaje) : IRequest<Unit>;
}
