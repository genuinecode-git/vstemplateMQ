namespace TemplateMQ.API.Application.Queries;

public interface ISampleQueries
{
    Task<IEnumerable<SampleViewModel>> GetSamplesAsync();
}
