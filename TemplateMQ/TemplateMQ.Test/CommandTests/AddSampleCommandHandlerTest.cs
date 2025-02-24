
namespace TemplateMQ.Test.CommandTests;

[TestFixture]
public class AddSampleCommandHandlerTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<AddSampleCommandHandler>> _mockLogger;
    private AddSampleCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AddSampleCommandHandler>>();

        _handler = new AddSampleCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object, _mockLogger.Object);
    }

    [Test]
    public void Constructor_NullUnitOfWork_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AddSampleCommandHandler(null, _mockMapper.Object, _mockLogger.Object));
    }

    [Test]
    public void Constructor_NullMapper_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AddSampleCommandHandler(_mockUnitOfWork.Object, null, _mockLogger.Object));
    }

    [Test]
    public void Constructor_NullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AddSampleCommandHandler(_mockUnitOfWork.Object, _mockMapper.Object, null));
    }

    [Test]
    public async Task Handle_SampleExists_ShouldReturnMappedSampleViewModel()
    {
        // Arrange
        var request = new AddSampleCommand(name : "ExistingSample");
        var sampleEntity = new Sample(request.Name);
        var sampleViewModel = new SampleViewModel { Name = request.Name };

        _mockUnitOfWork.Setup(u => u.Samples.FirstOrDefault(It.IsAny<Expression<Func<Sample, bool>>>()))
                .Returns(sampleEntity);

        _mockMapper.Setup(m => m.Map<SampleViewModel>(It.IsAny<Sample>()))
                   .Returns(sampleViewModel);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(request.Name));
        _mockUnitOfWork.Verify(u => u.Samples.Add(It.IsAny<Sample>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task Handle_SampleDoesNotExist_ShouldCreateAndReturnSampleViewModel()
    {
        // Arrange
        var request = new AddSampleCommand(name : "NewSample");
        var sampleEntity = new Sample(request.Name);
        var sampleViewModel = new SampleViewModel { Name = request.Name };

        _mockUnitOfWork.Setup(u => u.Samples.FirstOrDefault(It.IsAny<Expression<Func<Sample, bool>>>()))
               .Returns((Sample)null);

        _mockMapper.Setup(m => m.Map<SampleViewModel>(It.IsAny<Sample>()))
                   .Returns(sampleViewModel);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo(request.Name));
        _mockUnitOfWork.Verify(u => u.Samples.Add(It.IsAny<Sample>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}