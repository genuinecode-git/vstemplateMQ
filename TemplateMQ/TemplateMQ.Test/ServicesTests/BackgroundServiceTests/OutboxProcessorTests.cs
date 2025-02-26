namespace TemplateMQ.Test.ServicesTests.BackgroundServiceTests;

public class OutboxProcessorTests
{
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<IServiceScope> _mockServiceScope;
    private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private Mock<IOutboxService> _mockOutboxService;
    private Mock<IRabbitMqService> _mockRabbitMqService;
    private Mock<ILogger<OutboxProcessor>> _mockLogger;
    private OutboxProcessor _outboxProcessor;
    private CancellationTokenSource _cts;

    [SetUp]
    public void SetUp()
    {
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockServiceScope = new Mock<IServiceScope>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockOutboxService = new Mock<IOutboxService>();
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _mockLogger = new Mock<ILogger<OutboxProcessor>>();
        _cts = new CancellationTokenSource();

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_mockServiceScopeFactory.Object);
        _mockServiceScopeFactory.Setup(f => f.CreateScope())
            .Returns(_mockServiceScope.Object);
        _mockServiceScope.Setup(s => s.ServiceProvider)
            .Returns(_mockServiceProvider.Object);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IOutboxService)))
            .Returns(_mockOutboxService.Object);

        _outboxProcessor = new OutboxProcessor(_mockServiceProvider.Object, _mockRabbitMqService.Object, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Dispose of _outboxProcessor after each test to free up resources
        _outboxProcessor?.Dispose();
        _cts.Cancel();
        _cts.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldProcessMessages_WhenThereAreUnprocessedMessages()
    {
        // Arrange
        List<OutboxMessage> messages =
        [
            new OutboxMessage { Id = Guid.NewGuid(), Payload = "{ \"data\": \"test\" }", MessageType = "TestMessage" }
        ];

        _mockOutboxService.Setup(s => s.GetUnprocessedMessagesAsync())
            .ReturnsAsync(messages);

        _mockRabbitMqService.Setup(s => s.PublishMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockOutboxService.Setup(s => s.MarkAsProcessedAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var executeTask = _outboxProcessor.StartAsync(_cts.Token);
        await Task.Delay(100); // Allow some execution time
        _cts.Cancel();

        // Assert
        _mockRabbitMqService.Verify(s => s.PublishMessageAsync(messages[0].Payload, messages[0].MessageType,""), Times.Once);
        _mockOutboxService.Verify(s => s.MarkAsProcessedAsync(messages[0].Id), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_ShouldLogError_WhenPublishingFails()
    {
        // Arrange
        OutboxMessage message = new() { Id = Guid.NewGuid(), Payload = "{ \"data\": \"test\" }", MessageType = "TestMessage" };
        _mockOutboxService.Setup(s => s.GetUnprocessedMessagesAsync())
            .ReturnsAsync([message]);

        _mockRabbitMqService.Setup(s => s.PublishMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Publishing failed"));

        // Act
        var executeTask = _outboxProcessor.StartAsync(_cts.Token);
        await Task.Delay(100); // Allow some execution time
        _cts.Cancel();

        // Assert
        _mockLogger.Verify(logger =>
            logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => state.ToString().Contains("Failed to process outbox message")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}
