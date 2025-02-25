namespace TemplateMQ.API.Application.Services;

public interface IInboxService
{
    Task AddMessageAsync(InboxMessage message);
    Task<List<InboxMessage>> GetPendingMessagesAsync();
    Task MarkMessageAsProcessedAsync(Guid messageId);
    Task MoveToDeadLetterQueueAsync(Guid messageId);
}