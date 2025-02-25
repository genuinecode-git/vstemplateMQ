namespace TemplateMQ.Test.ServicesTests;

[TestFixture]
public class InboxServiceTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IInboxMessageRepository> _mockInboxMessages;
    private InboxService _inboxService;

    [SetUp]
    public void SetUp()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockInboxMessages = new Mock<IInboxMessageRepository>();
        _inboxService = new InboxService(_mockUnitOfWork.Object);

        _mockUnitOfWork.Setup(u => u.InboxMessages).Returns(_mockInboxMessages.Object);

    }

    [Test]
    public async Task AddMessageAsync_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Arrange
        InboxMessage nullMessage = null;

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _inboxService.AddMessageAsync(nullMessage));
    }

    [Test]
    public async Task AddMessageAsync_ShouldAddMessage_WhenMessageIsValid()
    {
        // Arrange
        var message = new InboxMessage { Id = Guid.NewGuid(), Payload = "Test message" };
        
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _inboxService.AddMessageAsync(message);

        // Assert
        _mockUnitOfWork.Verify(u => u.InboxMessages.Add(message), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task GetPendingMessagesAsync_ShouldReturnPendingMessages()
    {
        // Arrange
        List<InboxMessage> messages =
        [
            new() { Id = Guid.NewGuid(), Payload = "Message 1", ProcessedAt = null },
            new() { Id = Guid.NewGuid(), Payload = "Message 2", ProcessedAt = null }
        ];
        _mockUnitOfWork.Setup(u => u.InboxMessages.FilterAsync(It.IsAny<Expression<Func<InboxMessage, bool>>>()))
                       .ReturnsAsync(messages);

        // Act
        var result = await _inboxService.GetPendingMessagesAsync();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2));
        _mockUnitOfWork.Verify(u => u.InboxMessages.FilterAsync(It.IsAny<Expression<Func<InboxMessage, bool>>>()), Times.Once);
    }

    [Test]
    public async Task MarkMessageAsProcessedAsync_ShouldUpdateMessage_WhenMessageIsFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new InboxMessage { Id = messageId, Payload = "Test", ProcessedAt = null };
        _mockUnitOfWork.Setup(u => u.InboxMessages.FindAsync(messageId)).ReturnsAsync(message);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _inboxService.MarkMessageAsProcessedAsync(messageId);

        // Assert
        Assert.NotNull(message.ProcessedAt);
        _mockUnitOfWork.Verify(u => u.InboxMessages.Update(message), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task MarkMessageAsProcessedAsync_ShouldNotUpdate_WhenMessageNotFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.InboxMessages.FindAsync(messageId)).ReturnsAsync((InboxMessage)null);

        // Act
        await _inboxService.MarkMessageAsProcessedAsync(messageId);

        // Assert
        _mockUnitOfWork.Verify(u => u.InboxMessages.Update(It.IsAny<InboxMessage>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Test]
    public async Task MoveToDeadLetterQueueAsync_ShouldUpdateMessage_WhenMessageIsFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var message = new InboxMessage { Id = messageId, Payload = "Test", ProcessedAt = null };
        _mockUnitOfWork.Setup(u => u.InboxMessages.FindAsync(messageId)).ReturnsAsync(message);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _inboxService.MoveToDeadLetterQueueAsync(messageId);

        // Assert
        Assert.That(message.ErrorMessage, Is.EqualTo("DeadLetter"));
        _mockUnitOfWork.Verify(u => u.InboxMessages.Update(message), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Test]
    public async Task MoveToDeadLetterQueueAsync_ShouldNotUpdate_WhenMessageNotFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        _mockUnitOfWork.Setup(u => u.InboxMessages.FindAsync(messageId)).ReturnsAsync((InboxMessage)null);

        // Act
        await _inboxService.MoveToDeadLetterQueueAsync(messageId);

        // Assert
        _mockUnitOfWork.Verify(u => u.InboxMessages.Update(It.IsAny<InboxMessage>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
