namespace TemplateMQ.Infrastructure;

public interface IUnitOfWork
{
    ISampleRepository Samples { get; }
    IInboxMessageRepository InboxMessages { get; }
    IOutboxMessageRepository OutboxMessages { get; }

    Task SaveChangesAsync();
}
