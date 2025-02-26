namespace TemplateMQ.API.Application.Services;

public interface IRabbitMqService
{
    Task<IConnection> GetConnectionAsync();
    Task SetupQueueAsync();
    Task<IChannelWrapper> CreateChannelAsync();
    Task PublishMessageAsync(string message, string routingKey, string exchange = "");
    Task MoveToDeadLetterQueueAsync(InboxMessage message);
}
