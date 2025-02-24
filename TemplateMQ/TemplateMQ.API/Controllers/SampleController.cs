

namespace TemplateMQ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SampleController(IMediator mediator,ISampleQueries sampleQueries, ILogger<SampleController> logger) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly ISampleQueries _sampleQueries = sampleQueries;
    private readonly ILogger<SampleController> _logger = logger;

    [HttpGet("")]
    public async Task<IActionResult> GetSamplesAsync()
    {
        return Ok(await _sampleQueries.GetSamplesAsync());
    }

    [HttpPost("")]
    public async Task<IActionResult> AddSampleAsync([FromBody] AddSampleCommand command)
    {
        _logger.LogInformation("Received request to add Sample for {Name}", command.Name);

        var result = await _mediator.Send(command);
        return Ok(result);
    }


}
