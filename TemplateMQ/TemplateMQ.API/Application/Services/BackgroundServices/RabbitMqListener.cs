namespace TemplateMQ.API.Application.Services.BackgroundServices;

public class RabbitMqListener(
    IServiceProvider serviceProvider,
    IRabbitMqService rabbitMqService,
    ILogger<RabbitMqListener> logger) : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private IChannel _channel;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _rabbitMqService.SetupQueueAsync();
            var channelWrap = await _rabbitMqService.CreateChannelAsync();
            _channel = channelWrap.GetChannel();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await HandleMessageAsync(ea, stoppingToken);
            };

            await _channel.BasicConsumeAsync(queue: "main_queue", autoAck: false, consumer: consumer);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ listener encountered an error.");
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();

        _logger.LogInformation($"Received message: {messageId}");

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.InboxMessages.FindAsync(new object[] { messageId }, stoppingToken) != null)
        {
            _logger.LogInformation($"Duplicate message detected: {messageId}. Acknowledging...");
            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            return;
        }

        var inboxMessage = new InboxMessage
        {
            Id = Guid.Parse(messageId),
            MessageType = ea.BasicProperties?.Type ?? "",
            Payload = message,
            ReceivedAt = DateTime.UtcNow,
            RetryCount = 0
        };

        context.InboxMessages.Add(inboxMessage);
        await context.SaveChangesAsync(stoppingToken);

        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        base.Dispose();
    }
}