namespace TemplateMQ.Test.ControllerTests;

[TestFixture]
public class SampleControllerTests
{
    private Mock<IMediator> _mockMediator;
    private Mock<ISampleQueries> _mockSampleQueries;
    private Mock<ILogger<SampleController>> _mockLogger;
    private SampleController _controller;

    [SetUp]
    public void Setup()
    {
        _mockMediator = new Mock<IMediator>();
        _mockSampleQueries = new Mock<ISampleQueries>();
        _mockLogger = new Mock<ILogger<SampleController>>();

        _controller = new SampleController(_mockMediator.Object, _mockSampleQueries.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetSamplesAsync_ReturnsListOfSamples()
    {
        // Arrange
        var sampleList = new List<SampleViewModel>
            {
                new SampleViewModel { Id = 1, Name = "Sample 1" },
                new SampleViewModel { Id = 2, Name = "Sample 2" }
            };

        _mockSampleQueries.Setup(q => q.GetSamplesAsync()).ReturnsAsync(sampleList);

        // Act
        var result = await _controller.GetSamplesAsync() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.That(result.StatusCode, Is.EqualTo(200));
        Assert.That(result.Value, Is.EqualTo(sampleList));
    }

    [Test]
    public async Task AddSampleAsync_ValidCommand_ReturnsCreatedSample()
    {
        // Arrange
        AddSampleCommand command = new(name : "New Sample");
        var sampleViewModel = new SampleViewModel { Id = 1, Name = "New Sample" };

        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(sampleViewModel);

        // Act
        var result = await _controller.AddSampleAsync(command) as OkObjectResult;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(200));
            Assert.That(result.Value, Is.EqualTo(sampleViewModel));
        });

        // Verify logger
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Received request to add Sample")),
                null,
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
            ),
            Times.Once
        );
    }

    [Test]
    public void AddSampleAsync_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        AddSampleCommand command = new(name: "Error Sample");
        _mockMediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new System.Exception("Database error"));

        // Act
        var result = Assert.ThrowsAsync<Exception>(async () => await _controller.AddSampleAsync(command));

        // Assert
        Assert.That(result.Message, Is.EqualTo("Database error"));
    }
}
