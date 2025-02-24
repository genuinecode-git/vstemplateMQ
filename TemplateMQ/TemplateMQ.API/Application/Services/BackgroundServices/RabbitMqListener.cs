namespace TemplateMQ.API.Application.Services.BackgroundServices;

public class RabbitMqListener(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqSettings> rabbitMqSettings,
    Task<IConnection> connectionTask,
    ILogger<RabbitMqListener> logger) : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly RabbitMqSettings _rabbitMqSettings = rabbitMqSettings.Value;
    private IChannel? _channel;
    private IConnection? _connection;

    private readonly Task<IConnection> _connectionTask = connectionTask;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Await the connection task to ensure the connection is ready
        _connection = await _connectionTask;

        // Create the channel once the connection is established
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);  

        // Setup the queue configuration
        await SetupQueue();

        // Start consuming messages
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var messageId = Guid.NewGuid();  // For Idempotency checking

            _logger.LogInformation($"Received message: {messageId}");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Check for duplicate messages (Idempotency)
            if (await context.InboxMessages.AnyAsync(m => m.Id == messageId, stoppingToken))
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, false);  // Acknowledge duplicate message
                return;
            }

            // Store the message in Inbox table
            InboxMessage inboxMessage = new()
            {
                Id = messageId,
                MessageType = ea.BasicProperties.Type??"",
                Payload = message,
                ReceivedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            context.InboxMessages.Add(inboxMessage);
            await context.SaveChangesAsync(stoppingToken);

            // Acknowledge the message after processing
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };

        // Start listening on the main queue
        await _channel.BasicConsumeAsync(queue: _rabbitMqSettings.QueueName, autoAck: false, consumer: consumer);

        // Keep the service running until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    //public async Task MoveToDeadLetterQueueAsync(InboxMessage message)
    //{ 
    //    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
    //    await _channel.BasicPublishAsync(exchange: "", routingKey: "dead_letter_queue", body: body);
    //}

    private async Task SetupQueue()
    {
        // Declare the main queue and the dead-letter queue
        await _channel.QueueDeclareAsync(
            queue: _rabbitMqSettings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", _rabbitMqSettings.DeadLetterQueue }
            });

       await  _channel.QueueDeclareAsync(
            queue: _rabbitMqSettings.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}