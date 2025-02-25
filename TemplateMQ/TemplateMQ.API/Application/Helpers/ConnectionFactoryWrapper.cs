    namespace TemplateMQ.API.Application.Helpers;

public class ConnectionFactoryWrapper : IConnectionFactoryWrapper
{
    private readonly ConnectionFactory _connectionFactory;

    public ConnectionFactoryWrapper(RabbitMqSettings settings)
    {
        _connectionFactory = new ConnectionFactory
        {
            HostName = settings.HostName,
            UserName = settings.UserName,
            Password = settings.Password
        };
    }

    public async Task<IConnection> CreateConnectionAsync()
    {
        return await _connectionFactory.CreateConnectionAsync();
    }

    public async Task<IChannelWrapper> CreateChannelAsync()
    {
        var connection = await CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        return new ChannelWrapper(channel);
    }
}
