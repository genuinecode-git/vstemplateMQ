namespace TemplateMQ.API.Application.Services;

public class InboxService(IUnitOfWork unitOfWork) : IInboxService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task AddMessageAsync(InboxMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        _unitOfWork.InboxMessages.Add(message);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<InboxMessage>> GetPendingMessagesAsync()
    {
        return await _unitOfWork.InboxMessages.FilterAsync(m => m.ProcessedAt == null);
    }

    public async Task MarkMessageAsProcessedAsync(Guid messageId)
    {
        var message = await _unitOfWork.InboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.ProcessedAt = DateTime.UtcNow;
            _unitOfWork.InboxMessages.Update(message);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MoveToDeadLetterQueueAsync(Guid messageId)
    {
        InboxMessage message = await _unitOfWork.InboxMessages.FindAsync(messageId);
        if (message != null)
        {
            message.ErrorMessage = "DeadLetter";
            _unitOfWork.InboxMessages.Update(message);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
