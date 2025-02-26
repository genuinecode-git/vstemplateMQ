namespace TemplateMQ.API.Application.Services.BackgroundServices;

public class RabbitMqListener(
    IUnitOfWork unitOfWork,
    IRabbitMqService rabbitMqService,
    ILogger<RabbitMqListener> logger) : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger = logger;
    private readonly IUnitOfWork _unitOfWork =unitOfWork;
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private IChannel _channel;
    private IChannelWrapper _channelWrap;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _rabbitMqService.SetupQueueAsync();
            _channelWrap = await _rabbitMqService.CreateChannelAsync();
            _channel = _channelWrap.GetChannel();

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

    public async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        byte[] body = ea.Body.ToArray();
        string message = Encoding.UTF8.GetString(body);
        string messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
        
        if (_channel ==null)
        {
            _channelWrap = await _rabbitMqService.CreateChannelAsync();
            _channel = _channelWrap.GetChannel();
        }

        _logger.LogInformation($"Received message: {messageId}");

        if (await _unitOfWork.InboxMessages.FindAsync(messageId, stoppingToken) != null)
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

        _unitOfWork.InboxMessages.Add(inboxMessage);
        await _unitOfWork.SaveChangesAsync();

        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        base.Dispose();
    }
}