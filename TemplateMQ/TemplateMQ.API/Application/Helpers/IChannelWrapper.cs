namespace TemplateMQ.API.Application.Helpers;

public interface IChannelWrapper
{
    IChannel GetChannel();
    Task BasicPublishAsync(string exchange, string routingKey, ReadOnlyMemory<byte> body);
    Task QueueDeclareAsync(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object>? arguments);
}
