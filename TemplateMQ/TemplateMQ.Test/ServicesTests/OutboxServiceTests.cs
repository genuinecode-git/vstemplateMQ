namespace TemplateMQ.Test.ServicesTests;

[TestFixture]
public class OutboxServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IOutboxMessageRepository> _mockOutboxMessages;
    private OutboxService _outboxService;

    [SetUp]
    public void SetUp()
    {
        // Mock the OutboxMessages repository
        _mockOutboxMessages = new Mock<IOutboxMessageRepository>();

        // Mock the UnitOfWork
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.OutboxMessages).Returns(_mockOutboxMessages.Object);

        _outboxService = new OutboxService(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task AddMessageAsync_ShouldAddMessage_WhenMessageIsValid()
    {
        // Arrange
        var messageType = "TestMessageType";
        var payload = "TestPayload";
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _outboxService.AddMessageAsync(messageType, payload);

        // Assert
        _mockOutboxMessages.Verify(r => r.Add(It.Is<OutboxMessage>(m => m.MessageType == messageType && m.Payload == payload)), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetUnprocessedMessagesAsync_ShouldReturnUnprocessedMessages()
    {
        // Arrange
        List<OutboxMessage> messages =
        [
            new() { Id = Guid.NewGuid(), MessageType = "Test1", Payload = "Payload1", ProcessedAt = null },
            new() { Id = Guid.NewGuid(), MessageType = "Test2", Payload = "Payload2", ProcessedAt = null }
        ];
        _mockOutboxMessages.Setup(u => u.FilterAsync(It.IsAny<Expression<Func<OutboxMessage, bool>>>()))
                           .ReturnsAsync(messages);

        // Act
        var result = await _outboxService.GetUnprocessedMessagesAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        _mockOutboxMessages.Verify(u => u.FilterAsync(It.IsAny<Expression<Func<OutboxMessage, bool>>>()), Times.Once);
    }

    [Test]
    public async Task MarkAsProcessedAsync_ShouldUpdateMessage_WhenMessageIsFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new OutboxMessage { Id = messageId, MessageType = "Test", Payload = "Test Payload", ProcessedAt = null };
        _mockOutboxMessages.Setup(u => u.FindAsync(messageId)).ReturnsAsync(message);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _outboxService.MarkAsProcessedAsync(messageId);

        // Assert
        Assert.That(message.ProcessedAt, Is.Not.Null);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task MarkAsProcessedAsync_ShouldNotUpdate_WhenMessageNotFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        _mockOutboxMessages.Setup(u => u.FindAsync(messageId)).ReturnsAsync((OutboxMessage)null);

        // Act
        await _outboxService.MarkAsProcessedAsync(messageId);

        // Assert
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
