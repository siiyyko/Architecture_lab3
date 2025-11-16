using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using CommunicationService.EventDTOs;
using CommunicationService.Services;

namespace CommunicationService.Services
{
    public class MessageBusPublisher : IMessageBusPublisher, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly string _exchangeName = "task_events_exchange";

        public MessageBusPublisher(IConfiguration configuration)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = configuration["RabbitMQHost"],
                    Port = int.Parse(configuration["RabbitMQPort"])
                };

                _connection = factory.CreateConnectionAsync()
                                     .GetAwaiter()
                                     .GetResult();

                _channel = _connection.CreateChannelAsync()
                                      .GetAwaiter()
                                      .GetResult();

                _channel.ExchangeDeclareAsync(
                            exchange: _exchangeName,
                            type: "direct")
                        .GetAwaiter()
                        .GetResult();

                Console.WriteLine("--> (Publisher) Connected to RabbitMQ Message Bus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> (Publisher) Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        public void PublishTaskUpdated(TaskUpdatedEventDto taskUpdatedDto)
        {
            var message = JsonSerializer.Serialize(taskUpdatedDto);

            if (_connection != null && _connection.IsOpen)
            {
                var body = Encoding.UTF8.GetBytes(message);

                _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: "task.updated",
                    body: body)
                .GetAwaiter()
                .GetResult();

                Console.WriteLine($"--> (Publisher) We have sent: {message}");
            }
            else
            {
                Console.WriteLine("--> (Publisher) RabbitMQ connection is closed, not sending.");
            }
        }

        public void Dispose()
        {
            Console.WriteLine("--> (Publisher) Message Bus Disposed");
            try
            {
                if (_channel != null && _channel.IsOpen)
                {
                    _channel.CloseAsync().GetAwaiter().GetResult();
                }
                if (_connection != null && _connection.IsOpen)
                {
                    _connection.CloseAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> (Publisher) Error disposing RabbitMQ connection: {ex.Message}");
            }
        }
    }
}