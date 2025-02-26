using System.Threading;

namespace TemplateMQ.API.Application.Helpers;

public class ChannelWrapper(IChannel channel) : IChannelWrapper
{
    private readonly IChannel _channel = channel;

    public IChannel GetChannel()
    {
        return _channel;
    }
    public async Task BasicPublishAsync(string exchange, string routingKey, ReadOnlyMemory<byte> body)
    {
        await _channel.BasicPublishAsync(exchange, routingKey, body);
    }

    public async Task QueueDeclareAsync(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object>? arguments)
    {
        await _channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete, arguments);
    }
}