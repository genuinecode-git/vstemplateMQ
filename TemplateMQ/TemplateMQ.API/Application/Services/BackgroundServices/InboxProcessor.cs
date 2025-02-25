namespace TemplateMQ.API.Application.Services.BackgroundServices;

public class InboxProcessor(IMediator mediator, IServiceProvider _serviceProvider, ILogger<InboxProcessor> logger)
    : BackgroundService
{
    private readonly IMediator _mediator = mediator;
    private readonly ILogger<InboxProcessor> _logger = logger;

    private const int MaxRetries = 5; // Max retry attempts

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceProvider.CreateScope();
            IInboxService inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();
            var unprocessedMessages = await inboxService.GetPendingMessagesAsync();

            foreach (var message in unprocessedMessages)
            {
                if (message.RetryCount >= MaxRetries)
                {
                    _logger.LogWarning($"Message {message.Id} exceeded max retries. Moving to Dead-Letter Queue.");
                    await inboxService.MoveToDeadLetterQueueAsync(message.Id);
                    continue;
                }

                try
                {
                    Type? messageType = Type.GetType(message.MessageType);
                    if (messageType == null)
                    {
                        _logger.LogError($"Unknown message type: {message.MessageType}");
                        continue;
                    }

                    var command = Newtonsoft.Json.JsonConvert.DeserializeObject(message.Payload, messageType);
                    if (command == null) continue;

                    await _mediator.Send(command, stoppingToken);
                    await inboxService.MarkMessageAsProcessedAsync(message.Id);
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.ErrorMessage = ex.Message;

                    _logger.LogError(ex, $"Error processing message {message.Id}");

                    if (message.RetryCount >= MaxRetries)
                    {
                        _logger.LogWarning($"Message {message.Id} moved to Dead-Letter Queue");
                        await inboxService.MoveToDeadLetterQueueAsync(message.Id);
                    }
                    else
                    {
                        _logger.LogWarning($"Retrying message {message.Id}, Attempt: {message.RetryCount}");
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
