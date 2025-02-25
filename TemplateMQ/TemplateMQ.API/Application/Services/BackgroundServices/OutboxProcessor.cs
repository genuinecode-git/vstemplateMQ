namespace TemplateMQ.API.Application.Services.BackgroundServices;

public class OutboxProcessor(IServiceProvider serviceProvider, IRabbitMqService rabbitMqService, ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
    private readonly ILogger<OutboxProcessor> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            IOutboxService outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();
            var messages = await outboxService.GetUnprocessedMessagesAsync();

            foreach (var message in messages)
            {
                try
                {
                    await _rabbitMqService.PublishMessageAsync(message.Payload, message.MessageType);
                    await outboxService.MarkAsProcessedAsync(message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process outbox message: {MessageId}", message.Id);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Poll every 5 seconds
        }
    }
}

