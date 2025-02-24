namespace TemplateMQ.Infrastructure.Repositories;

public class SampleRepository(ApplicationDbContext context) : Repository<Sample>(context), ISampleRepository
{

}
