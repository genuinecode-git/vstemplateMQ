using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System.Threading.Channels;
using TemplateMQ.API.Application.Helpers;
using TemplateMQ.API.Application.Models.ConfigurationModels;
using TemplateMQ.API.Application.Services;
using TemplateMQ.Domain.Models;

namespace TemplateMQ.Test.ServicesTests;

[TestFixture]
public class RabbitMqServiceTests
{
    private Mock<IOptions<RabbitMqSettings>> _mockOptions;
    private Mock<IConnectionFactoryWrapper> _mockConnectionFactoryWrapper;
    private Mock<IConnection> _mockConnection;
    private Mock<IChannelWrapper> _mockChannelWrapper;
    private RabbitMqService _rabbitMqService;

    [SetUp]
    public void SetUp()
    {
        // Mock RabbitMQ settings
        var settings = new RabbitMqSettings
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            QueueName = "test_queue",
            DeadLetterQueue = "test_dlq"
        };

        _mockOptions = new Mock<IOptions<RabbitMqSettings>>();
        _mockOptions.Setup(o => o.Value).Returns(settings);

        // Mock RabbitMQ connection and channel
        _mockConnection = new Mock<IConnection>();
        _mockChannelWrapper = new Mock<IChannelWrapper>();

        _mockChannelWrapper
            .Setup(s => s.QueueDeclareAsync(It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<Dictionary<string, object>>()
            )).Returns(Task.CompletedTask);

        // Mock CreateChannelAsync to return the mocked channel
        _mockConnectionFactoryWrapper = new Mock<IConnectionFactoryWrapper>();
        _mockConnectionFactoryWrapper
            .Setup(f => f.CreateConnectionAsync())
            .ReturnsAsync(_mockConnection.Object);

        _mockConnectionFactoryWrapper
           .Setup(c => c.CreateChannelAsync())
           .ReturnsAsync(_mockChannelWrapper.Object);

        // Initialize the service
        _rabbitMqService = new RabbitMqService(_mockOptions.Object, _mockConnectionFactoryWrapper.Object);
    }

    [Test]
    public async Task GetConnectionAsync_ShouldReturnConnection()
    {
        // Act
        var connection = await _rabbitMqService.GetConnectionAsync();

        // Assert
        Assert.That(connection, Is.Not.Null);
        Assert.That(connection, Is.EqualTo(_mockConnection.Object));
    }

    [Test]
    public async Task CreateChannelAsync_ShouldReturnChannel()
    {
        // Act
        IChannelWrapper channel = await _rabbitMqService.CreateChannelAsync();

        // Assert
        Assert.That(channel, Is.Not.Null);
    }

    [Test]
    public async Task SetupQueueAsync_ShouldDeclareQueues()
    {
        // Act
        await _rabbitMqService.SetupQueueAsync();

        // Assert
        _mockChannelWrapper.Verify(c => c.QueueDeclareAsync(
            _mockOptions.Object.Value.QueueName,
            true,
            false,
            false,
            It.IsAny<Dictionary<string, object>>()), Times.Once);

        _mockChannelWrapper.Verify(c => c.QueueDeclareAsync(
            _mockOptions.Object.Value.DeadLetterQueue,
            true,
            false,
            false,
            null), Times.Once);
    }

    [Test]
    public async Task PublishMessageAsync_ShouldPublishMessage()
    {
        // Arrange
        var message = "test_message";
        var routingKey = "test_routing_key";

        // Act
        await _rabbitMqService.PublishMessageAsync(message, routingKey);

        // Assert
        _mockChannelWrapper.Verify(c => c.BasicPublishAsync(
            "",
            routingKey,
            It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
    }

    [Test]
    public async Task MoveToDeadLetterQueueAsync_ShouldPublishToDeadLetterQueue()
    {
        // Arrange
        var message = new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "TestMessage",
            Payload = "TestPayload",
            ReceivedAt = DateTime.UtcNow,
            RetryCount = 0
        };

        // Act
        await _rabbitMqService.MoveToDeadLetterQueueAsync(message);

        // Assert
        _mockChannelWrapper.Verify(c => c.BasicPublishAsync(
            "",
            _mockOptions.Object.Value.DeadLetterQueue,
            It.IsAny<ReadOnlyMemory<byte>>()), Times.Once);
    }
}