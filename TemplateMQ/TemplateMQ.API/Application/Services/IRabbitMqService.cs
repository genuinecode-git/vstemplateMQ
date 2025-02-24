namespace TemplateMQ.API.Application.Services;

public interface IRabbitMqService
{
    Task MoveToDeadLetterQueueAsync(InboxMessage message);
}
