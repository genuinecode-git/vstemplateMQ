
namespace TemplateMQ.API.Application.Queries;

public class SampleQueries(IDbConnection connection) :BaseQuery(connection), ISampleQueries
{
    public async Task<IEnumerable<SampleViewModel>> GetSamplesAsync()
    {
        string sql = "SELECT Id, Name FROM Samples";
        return await _connection.QueryAsync<SampleViewModel>(sql);

    }
}
