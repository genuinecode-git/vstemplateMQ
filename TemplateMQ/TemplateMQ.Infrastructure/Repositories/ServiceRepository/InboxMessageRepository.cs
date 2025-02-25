
namespace TemplateMQ.Infrastructure.Repositories.ServiceRepository;

public class InboxMessageRepository(ApplicationDbContext context) : Repository<InboxMessage>(context), IInboxMessageRepository
{

}
