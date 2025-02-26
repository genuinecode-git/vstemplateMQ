namespace TemplateMQ.Test.ServicesTests.BackgroundServiceTests;

[TestFixture]
public class InboxProcessorTests
{
    private Mock<IMediator> _mockMediator;
    private Mock<IServiceProvider> _mockServiceProvider;
    private Mock<IInboxService> _mockInboxService;
    private Mock<ILogger<InboxProcessor>> _mockLogger;
    private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private Mock<IServiceScope> _mockServiceScope;
    private InboxProcessor _inboxProcessor;

    [SetUp]
    public void SetUp()
    {
        _mockMediator = new Mock<IMediator>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockInboxService = new Mock<IInboxService>();
        _mockLogger = new Mock<ILogger<InboxProcessor>>();
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockServiceScope = new Mock<IServiceScope>();

        // Mock IServiceScopeFactory to return a mocked IServiceScope
        _mockServiceScopeFactory.Setup(factory => factory.CreateScope()).Returns(_mockServiceScope.Object);

        // Mock IServiceScope to return IServiceProvider that returns IInboxService
        _mockServiceScope.Setup(scope => scope.ServiceProvider.GetService(typeof(IInboxService))).Returns(_mockInboxService.Object);

        // Mock IServiceProvider to return the IServiceScopeFactory
        _mockServiceProvider.Setup(s => s.GetService(typeof(IServiceScopeFactory))).Returns(_mockServiceScopeFactory.Object);

        // Instantiate InboxProcessor
        _inboxProcessor = new InboxProcessor(_mockMediator.Object, _mockServiceProvider.Object, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Dispose of _inboxProcessor after each test to free up resources
        _inboxProcessor?.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldProcessMessages_WhenThereAreUnprocessedMessages()
    {
        // Arrange
        InboxMessage message = new()
        {
            Id = Guid.NewGuid(),
            MessageType = "TemplateMQ.API.Application.Commands.AddSampleCommand, TemplateMQ.API",
            Payload = "{\"Name\":\"SomeValue\"}",
            RetryCount = 0
        };

        List<InboxMessage> unprocessedMessages = [message];
        _mockInboxService.Setup(service => service.GetPendingMessagesAsync()).ReturnsAsync(unprocessedMessages);
        _mockMediator.Setup(mediator => mediator.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(Unit.Value);
        _mockInboxService.Setup(service => service.MarkMessageAsProcessedAsync(message.Id)).Returns(Task.CompletedTask);

        // Act - Using reflection to invoke the protected ExecuteAsync method
        CancellationTokenSource cancellationTokenSource = new();
        var cancellationToken = cancellationTokenSource.Token;

        var executeAsyncMethod = typeof(InboxProcessor).GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var executeTask = (Task)executeAsyncMethod.Invoke(_inboxProcessor, [cancellationToken]);


        // Cancel the token after 1 second to stop the infinite loop
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

        // Wait for the task to complete
        await Task.WhenAny(executeTask, Task.Delay(TimeSpan.FromSeconds(1))); // Ensure the test doesn't hang indefinitely


        // Assert
        _mockInboxService.Verify(service => service.GetPendingMessagesAsync(), Times.Once);
        _mockMediator.Verify(mediator => mediator.Send(It.IsAny<object>(), cancellationToken), Times.Once);
        _mockInboxService.Verify(service => service.MarkMessageAsProcessedAsync(message.Id), Times.Once);

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),  
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString().Contains($"Processing message {message.MessageType}") // Ensure correct log message
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>() 
            ),
            Times.Once 
        );
    }

    [Test]
    public async Task ExecuteAsync_ShouldMoveMessageToDeadLetterQueue_WhenMaxRetriesExceeded()
    {
        // Arrange
        InboxMessage message = new()
        {
            Id = Guid.NewGuid(),
            MessageType = "TemplateMQ.API.Application.Commands.AddSampleCommand, TemplateMQ.API",
            Payload = "{\"Name\":\"SomeValue\"}",
            RetryCount = 5
        };

        List<InboxMessage> unprocessedMessages = [message];
        _mockInboxService.Setup(service => service.GetPendingMessagesAsync()).ReturnsAsync(unprocessedMessages);
        _mockInboxService.Setup(service => service.MoveToDeadLetterQueueAsync(message.Id)).Returns(Task.CompletedTask);

        // Act - Using reflection to invoke the protected ExecuteAsync method
        CancellationTokenSource cancellationTokenSource = new();
        var cancellationToken = cancellationTokenSource.Token;

        var executeAsyncMethod = typeof(InboxProcessor).GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var executeTask = (Task)executeAsyncMethod.Invoke(_inboxProcessor, [cancellationToken]);

        // Cancel the token after 1 second to stop the infinite loop
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

        // Wait for the task to complete
        await Task.WhenAny(executeTask, Task.Delay(TimeSpan.FromSeconds(1))); // Ensure the test doesn't hang indefinitely

        // Assert
        _mockInboxService.Verify(service => service.MoveToDeadLetterQueueAsync(message.Id), Times.Once);
        _mockInboxService.Verify(service => service.MarkMessageAsProcessedAsync(It.IsAny<Guid>()), Times.Never); // Not processed because max retries exceeded

        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Warning, 
                It.IsAny<EventId>(),  
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString().Contains($"Message {message.Id} exceeded max retries. Moving to Dead-Letter Queue.") // Match exact log message
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ),
            Times.Once 
        );
    }

    [Test]
    public async Task ExecuteAsync_ShouldRetryMessage_WhenErrorOccurs()
    {
        // Arrange
        InboxMessage message = new()
        {
            Id = Guid.NewGuid(),
            MessageType = "TemplateMQ.API.Application.Commands.AddSampleCommand, TemplateMQ.API",
            Payload = "{\"Name\":\"SomeValue\"}",
            RetryCount = 0
        };

        List<InboxMessage> unprocessedMessages = [message];
        _mockInboxService.Setup(service => service.GetPendingMessagesAsync()).ReturnsAsync(unprocessedMessages);
        _mockMediator.Setup(mediator => mediator.Send(It.IsAny<object>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Test exception"));
        _mockInboxService.Setup(service => service.MarkMessageAsProcessedAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        // Act - Using reflection to invoke the protected ExecuteAsync method
        CancellationTokenSource cancellationTokenSource = new();
        var cancellationToken = cancellationTokenSource.Token;

        var executeAsyncMethod = typeof(InboxProcessor).GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        var executeTask = (Task)executeAsyncMethod.Invoke(_inboxProcessor, [cancellationToken]);

        // Cancel the token after 1 second to stop the infinite loop
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));

        // Wait for the task to complete
        await Task.WhenAny(executeTask, Task.Delay(TimeSpan.FromSeconds(1))); // Ensure the test doesn't hang indefinitely

        // Assert
        _mockInboxService.Verify(service => service.MarkMessageAsProcessedAsync(It.IsAny<Guid>()), Times.Never); // Not processed
        _mockInboxService.Verify(service => service.MoveToDeadLetterQueueAsync(It.IsAny<Guid>()), Times.Never); // Not moved to dead letter queue yet
        _mockLogger.Verify(
            logger => logger.Log(
                LogLevel.Error, 
                It.IsAny<EventId>(),  
                It.Is<It.IsAnyType>((state, type) =>
                    state.ToString().Contains($"Error processing message {message.Id}") // Match exact log message
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>() 
            ),
            Times.Once 
        );
    }
}