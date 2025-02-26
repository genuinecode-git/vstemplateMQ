namespace TemplateMQ.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    public ISampleRepository Samples { get; }
    public IInboxMessageRepository InboxMessages { get; }
    public IOutboxMessageRepository OutboxMessages { get; }

    public UnitOfWork(ApplicationDbContext context,
        ISampleRepository sampleRepository,
        IInboxMessageRepository _inboxMessages,
        IOutboxMessageRepository _outboxMessages)
    {
        this._context = context;
        Samples = sampleRepository;
        InboxMessages = _inboxMessages;
        OutboxMessages = _outboxMessages;
    }

    public async Task SaveChangesAsync()
    {
        await this._context.SaveChangesAsync();
    }

    public void Dispose()
    {
        this._context.Dispose();
    }
}
