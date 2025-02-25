
namespace TemplateMQ.API.Application.Services;

public class RabbitMqService : IRabbitMqService
{
    private readonly RabbitMqSettings _settings;
    private readonly IConnectionFactoryWrapper _connectionFactoryWrapper;
    private readonly Task<IConnection> _connectionTask;

    public RabbitMqService(IOptions<RabbitMqSettings> options, IConnectionFactoryWrapper connectionFactoryWrapper)
    {
        _settings = options.Value;
        _connectionFactoryWrapper = connectionFactoryWrapper;
        _connectionTask = _connectionFactoryWrapper.CreateConnectionAsync();
    }

    public async Task<IConnection> GetConnectionAsync()
    {
        return await _connectionTask;
    }

    public async Task<IChannelWrapper> CreateChannelAsync()
    {
        return await _connectionFactoryWrapper.CreateChannelAsync();
    }

    public async Task SetupQueueAsync()
    {
        IChannelWrapper channel = await CreateChannelAsync();
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
       await channel.QueueDeclareAsync(
            queue: _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", _settings.DeadLetterQueue }
            });
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        await channel.QueueDeclareAsync(
            queue: _settings.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task PublishMessageAsync(string message, string routingKey, string exchange = "")
    {
        var channel = await CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            body: body);
    }
   
    public async Task MoveToDeadLetterQueueAsync(InboxMessage message)
    {
        var channel = await CreateChannelAsync();
        
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: _settings.DeadLetterQueue,
            body: body);
    }
}
