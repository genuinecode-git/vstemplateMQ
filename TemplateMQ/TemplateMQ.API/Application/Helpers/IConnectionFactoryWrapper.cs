namespace TemplateMQ.API.Application.Helpers;

public interface IConnectionFactoryWrapper
{
    Task<IConnection> CreateConnectionAsync();
    Task<IChannelWrapper> CreateChannelAsync();
}
