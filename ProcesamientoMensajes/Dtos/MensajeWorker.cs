using ProcesamientoMensajes.Enumeradores;
using System.Text.Json.Serialization;

namespace ProcesamientoMensajes.Dtos
{
    public class MensajeWorker<T> where T : class
    {
        [JsonPropertyName("tipoComando")]
        public TipoComandoEnumeracion TipoComando { get; set; }

        [JsonPropertyName("contenido")]
        public T Contenido { get; set; } = default!;
    }
}
