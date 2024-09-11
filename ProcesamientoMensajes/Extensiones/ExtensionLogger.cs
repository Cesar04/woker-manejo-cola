using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;

namespace ProcesamientoMensajes.Extensiones
{
    public static class ExtensionLogger
    {
        public static ILoggingBuilder AgregarLogger(this ILoggingBuilder logging, IConfiguration configuration)
        {
            logging.ClearProviders();

            ConfigurarLogger(configuration);

            logging.AddSerilog(Log.Logger);

            return logging;
        }
        private static void ConfigurarLogger(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                .CreateLogger();
        }
    }
}
