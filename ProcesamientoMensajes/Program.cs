
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcesamientoMensajes;
using ProcesamientoMensajes.Extensiones;
using ProcesamientoMensajes.Procesadores;
using Serilog;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var config = builder.Configuration;

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Logging.AgregarLogger(config);
builder.Services.AddSingleton(Log.Logger);

#region Configuración servicios de RabbitMQ
builder.Services.AgregarProcesadorColaRabbitMQ(config);
builder.Services.AddSingleton<ProcesadorMensajeRabbitMQ>();
#endregion

#region Configuración servicios de Service bus
//builder.Services.AddSingleton<ProcesadorMensajeServiceBus>();
//builder.Services.AgregarProcesadorColaServiceBus(config);
#endregion

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();
