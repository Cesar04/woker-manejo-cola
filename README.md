# Mensajería con Azure Service Bus y RabbitMQ en .NET 8

Este proyecto demuestra cómo implementar el procesamiento de mensajes mediante colas utilizando dos tecnologías diferentes: Azure Service Bus y RabbitMQ, en .NET 8.

## Requisitos

Antes de comenzar, asegúrate de tener instalados los siguientes componentes:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [RabbitMQ](https://www.rabbitmq.com/download.html)

## Configuración del Ambiente

### Azure Service Bus

1. **Crear un namespace de Azure Service Bus**:
   - Ve al portal de Azure y crea un nuevo namespace de Service Bus.
   - Crea una cola dentro del namespace.

2. **Obtener la cadena de conexión**:
   - En el portal de Azure, navega hasta tu namespace de Service Bus y copia la cadena de conexión.

3. **Configurar la cadena de conexión en el proyecto**:
   - Abre el archivo `appsettings.json` y añade tu cadena de conexión y el nombre de la cola:

   ```json
   {
     "Cola": {
       "ConnectionString": "your_service_bus_connection_string",
       "Nombre": "your_queue_name"
     }
   }

### RabbitMQ

1. **Descargar los programas necesarios**:
   - **Erlang**: [Descargar Erlang](https://www.erlang.org/downloads), que es un requisito para el correcto funcionamiento de RabbitMQ.
   - **RabbitMQ**: [Descargar RabbitMQ](https://www.rabbitmq.com/docs/install-windows#downloads).

2. **Ejecutar RabbitMQ**:
   - Abre la consola de comandos y ejecuta el siguiente comando para iniciar RabbitMQ:
     ```
     net start RabbitMQ
     ```
   - Asegúrate de agregar RabbitMQ a la variable de entorno PATH.
   - Cierra y vuelve a abrir la ventana de comandos para que los cambios surtan efecto.
   - Habilita la interfaz de administración ejecutando:
     ```
     rabbitmq-plugins enable rabbitmq_management
     ```
   - Accede a la interfaz de administración en [http://localhost:15672](http://localhost:15672).
   - Inicia sesión con las siguientes credenciales:
     - **Usuario**: `guest`
     - **Contraseña**: `guest`
   - Crea una cola con el nombre **"generar-mensaje-cola"**.
   - Una vez dentro de la cola, publica un mensaje utilizando la estructura que se muestra a continuación.

3. **Ejemplo de estructura para encolar un mensaje**:
   ```json
   {
     "tipoComando": 0,
     "contenido": { 
       "Mensaje": "Mensaje para mostrar en consola" 
     }
   }
