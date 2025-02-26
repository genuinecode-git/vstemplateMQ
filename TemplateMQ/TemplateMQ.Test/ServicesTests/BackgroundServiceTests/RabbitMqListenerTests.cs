using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using TemplateMQ.API.Application.Helpers;

namespace TemplateMQ.Test.ServicesTests.BackgroundServiceTests;

[TestFixture]
public class RabbitMqListenerTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IRabbitMqService> _rabbitMqServiceMock;
    private Mock<ILogger<RabbitMqListener>> _loggerMock;
    private Mock<IChannelWrapper> _channelWrapperMock;
    private Mock<IChannel> _channelMock;
    private RabbitMqListener _rabbitMqListener;
    [SetUp]
    public void SetUp()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _rabbitMqServiceMock = new Mock<IRabbitMqService>();
        _loggerMock = new Mock<ILogger<RabbitMqListener>>();
        _channelMock = new Mock<IChannel>();
        _channelWrapperMock = new Mock<IChannelWrapper>();

        _channelWrapperMock.Setup(c => c.GetChannel()).Returns(_channelMock.Object);

        _rabbitMqServiceMock
            .Setup(x => x.CreateChannelAsync())
            .ReturnsAsync(_channelWrapperMock.Object);


        _rabbitMqListener = new RabbitMqListener(
            _unitOfWorkMock.Object,
            _rabbitMqServiceMock.Object,
            _loggerMock.Object
        );
    }
    [TearDown]
    public void TearDown()
    {
        // Dispose of _rabbitMqListener after each test to free up resources
        _rabbitMqListener?.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldSetupQueueAndCreateChannel()
    {
        // Arrange        
        _rabbitMqServiceMock.Setup(r => r.SetupQueueAsync()).Returns(Task.CompletedTask);

        var cancellationTokenSource = new CancellationTokenSource();

        // Act
        await _rabbitMqListener.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(100); // Allow some time for the async method to execute

        // Assert
        _rabbitMqServiceMock.Verify(r => r.SetupQueueAsync(), Times.Once);
        _rabbitMqServiceMock.Verify(r => r.CreateChannelAsync(), Times.Once);
        _channelMock.Verify(c => c.BasicConsumeAsync(
           "main_queue",
           false,
           It.IsAny<string>(),
           false,
           false,
           null,
           It.IsAny<AsyncEventingBasicConsumer>(),
           It.IsAny<CancellationToken>()
       ), Times.Once);
    }

    [Test]
    public async Task HandleMessageAsync_ShouldHandleDuplicateMessage()
    {
        // Arrange
        string messageId = Guid.NewGuid().ToString();
        var body = Encoding.UTF8.GetBytes("Test Message");
        BasicProperties basicPropertiesMock = new()
        {
            MessageId = messageId
        };
        Mock<IChannelWrapper> channelWrapperMock = new();
        channelWrapperMock.Setup(c => c.GetChannel()).Returns(_channelMock.Object);

        BasicDeliverEventArgs ea = new(
            consumerTag: "test-consumer-tag",
            deliveryTag: 1,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: basicPropertiesMock,
            body: new ReadOnlyMemory<byte>(body),
            cancellationToken: CancellationToken.None
        );

        _unitOfWorkMock.Setup(u => u.InboxMessages.FindAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InboxMessage { Id = Guid.Parse(messageId) });

        RabbitMqListener rabbitMqListener = new(
            _unitOfWorkMock.Object,
            _rabbitMqServiceMock.Object,
            _loggerMock.Object
        );
        // Act
        await rabbitMqListener.HandleMessageAsync(ea, CancellationToken.None);

        // Assert
        _channelMock.Verify(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(logger =>
               logger.Log(
                   LogLevel.Information,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((state, type) => state.ToString().Contains($"Duplicate message detected: {messageId}. Acknowledging...")),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()
               ), Times.Once);
    }

    [Test]
    public async Task HandleMessageAsync_ShouldHandleNewMessage()
    {
        // Arrange
        string messageId = Guid.NewGuid().ToString();
        byte[] body = Encoding.UTF8.GetBytes("Test Message");
        BasicProperties basicPropertiesMock = new()
        {
            MessageId = messageId,
            Type = "TestType"
        };

        var ea = new BasicDeliverEventArgs(
            consumerTag: "test-consumer-tag",
            deliveryTag: 1,
            redelivered: false,
            exchange: "test-exchange",
            routingKey: "test-routing-key",
            properties: basicPropertiesMock,
            body: new ReadOnlyMemory<byte>(body),
            cancellationToken: CancellationToken.None
        );

        _unitOfWorkMock.Setup(u => u.InboxMessages.FindAsync(new object[] { Guid.Parse(messageId) }, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InboxMessage)null);

        // Act
        await _rabbitMqListener.HandleMessageAsync(ea, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.InboxMessages.Add(It.Is<InboxMessage>(m => m.Id == Guid.Parse(messageId))), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        _channelMock.Verify(c => c.BasicAckAsync(1, false, It.IsAny<CancellationToken>()), Times.Once);
    }

}