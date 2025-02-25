namespace TemplateMQ.API.Application.Services;

public interface IOutboxService
{
    Task AddMessageAsync(string messageType, string payload);
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync();
    Task MarkAsProcessedAsync(Guid messageId);
}
