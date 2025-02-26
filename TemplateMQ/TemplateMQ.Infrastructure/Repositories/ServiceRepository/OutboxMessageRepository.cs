
namespace TemplateMQ.Infrastructure.Repositories.ServiceRepository;

public class OutboxMessageRepository(ApplicationDbContext context) : Repository<OutboxMessage>(context), IOutboxMessageRepository
{

}
