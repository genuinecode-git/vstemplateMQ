namespace TemplateMQ.API.Application.Services.BackgroundServices;

public class InboxProcessor(IMediator mediator, ApplicationDbContext context,ILogger<InboxProcessor> logger) : BackgroundService
{
    private readonly IMediator _mediator = mediator;
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<InboxProcessor> _logger = logger;

    private const int MaxRetries = 5; // Max retry attempts

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            var unprocessedMessages = await context.InboxMessages
                .Where(m => m.ProcessedAt == null && (m.RetryCount == 0 || m.RetryCount < MaxRetries))
                .ToListAsync(stoppingToken);

            foreach (var message in unprocessedMessages)
            {
                try
                {
                    // Deserialize event/command
                    Type? messageType = Type.GetType(message.MessageType);
                    if (messageType == null) continue;

                    var command = Newtonsoft.Json.JsonConvert.DeserializeObject(message.Payload, messageType);
                    if (command == null) continue;

                    // Send command to MediatR
                    await _mediator.Send(command, stoppingToken);

                    // Mark as processed
                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.ErrorMessage = ex.Message;
                    if (message.RetryCount >= MaxRetries)
                    {
                        Console.WriteLine($"Message {message.Id} moved to Dead-Letter Queue");
                        _logger.LogInformation($"Message is out of max retry count :{message.Id}");

                        //Moveto DLQ need impliment 
                    }

                    Console.WriteLine($"Error processing inbox message: {ex.Message}");
                    _logger.LogError(ex, "Error processing inbox");
                }
                finally
                {
                    await _context.SaveChangesAsync(stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
