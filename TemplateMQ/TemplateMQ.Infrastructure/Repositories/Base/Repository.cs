namespace TemplateMQ.Infrastructure.Repositories.Base;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public IQueryable<T> GetAll()
    {
        return _dbSet.AsQueryable();
    }

    public T FirstOrDefault(Expression<Func<T, bool>> predicate)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return _dbSet.FirstOrDefault(predicate);
#pragma warning restore CS8603 // Possible null reference return.
    }

    public T FirstOrDefaultWithIncludes(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        // Apply includes
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

#pragma warning disable CS8603 // Possible null reference return.
        return query.FirstOrDefault(predicate);
#pragma warning restore CS8603 // Possible null reference return.
    }

    public void Add(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Add(entity);
        _context.SaveChanges();
    }

    public void Update(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Update(entity);
        _context.SaveChanges();
    }

    public void Remove(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }
}
