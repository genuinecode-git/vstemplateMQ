namespace TemplateMQ.API.Application.Commands;

public class AddSampleCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<AddSampleCommandHandler> logger) : IRequestHandler<AddSampleCommand, SampleViewModel>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ILogger<AddSampleCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<SampleViewModel> Handle(AddSampleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Start] Processing AddSampleCommand for : {Name}", request.Name);

        Sample sample = _unitOfWork.Samples.FirstOrDefault(a => a.Name == request.Name);

        if (sample == null)
        {
            _logger.LogInformation("[Processing] Creating new Sample for : {Name}", request.Name);

            sample = new Sample(request.Name);
            _unitOfWork.Samples.Add(sample);

            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<SampleViewModel>(sample);
    }
}
