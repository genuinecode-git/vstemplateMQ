namespace TemplateMQ.Infrastructure;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _context;
    public ISampleRepository Samples { get; }

    public UnitOfWork(ApplicationDbContext context, ISampleRepository sampleRepository)
    {
        this._context = context;
        Samples = sampleRepository;
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
