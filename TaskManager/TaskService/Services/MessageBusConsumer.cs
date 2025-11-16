using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskService.Data;
using TaskService.EventDTOs;

namespace TaskService.Services
{
    public class MessageBusConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IChannel _channel;
        private readonly string _exchangeName = "task_events_exchange";
        private readonly string _queueName = "task_update_queue";

        public MessageBusConsumer(IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _scopeFactory = scopeFactory;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configuration["RabbitMQHost"],
                    Port = int.Parse(_configuration["RabbitMQPort"])
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

                _channel.QueueDeclareAsync(
                            queue: _queueName,
                            durable: true,
                            exclusive: false,
                            autoDelete: false)
                        .GetAwaiter()
                        .GetResult();

                _channel.QueueBindAsync(
                            queue: _queueName,
                            exchange: _exchangeName,
                            routingKey: "task.updated")
                        .GetAwaiter()
                        .GetResult();

                Console.WriteLine("--> (Consumer) Connected to RabbitMQ and listening.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> (Consumer) Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"--> (Consumer) Received message: {message}");

                try
                {
                    ProcessEvent(message);

                    _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> (Consumer) Error processing message: {ex.Message}");
                    // TODO: Тут можна додати логіку 'BasicNack' для повторної обробки
                }
            };

            await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private void ProcessEvent(string message)
        {
            var eventData = JsonSerializer.Deserialize<TaskUpdatedEventDto>(message);

            if (eventData == null)
            {
                Console.WriteLine("--> (Consumer) Could not deserialize message.");
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TaskServiceContext>();

                var task = dbContext.TaskEntity.Find(eventData.TaskId);

                if (task != null)
                {
                    task.LastUpdated = eventData.Timestamp;
                    dbContext.SaveChanges();
                    Console.WriteLine($"--> (Consumer) Task {task.Id} updated.");
                }
                else
                {
                    Console.WriteLine($"--> (Consumer) Task {eventData.TaskId} not found.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null && _channel.IsOpen)
            {
                await _channel.CloseAsync();
            }
            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
