namespace TemplateMQ.API.Application.Services;

public class OutboxService(IUnitOfWork unitOfWork) : IOutboxService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task AddMessageAsync(string messageType, string payload)
    {
        var message = new OutboxMessage
        {
            MessageType = messageType,
            Payload = payload
        };

        _unitOfWork.OutboxMessages.Add(message);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync()
    {
        return await _unitOfWork.OutboxMessages
            .FilterAsync(x => x.ProcessedAt == null);
    }

    public async Task MarkAsProcessedAsync(Guid messageId)
    {
        var message = await _unitOfWork.OutboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

