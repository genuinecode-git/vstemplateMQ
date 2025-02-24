
namespace TemplateMQ.API.Application.Services
{
    public class RabbitMqService (
    Task<IConnection> connectionTask) : IRabbitMqService
    {
        private IChannel? _channel;
        private IConnection? _connection;
        private readonly Task<IConnection> _connectionTask = connectionTask;


        public async Task MoveToDeadLetterQueueAsync(InboxMessage message)
        {
            // Await the connection task to ensure the connection is ready
            _connection = await _connectionTask;

            // Create the channel once the connection is established
            _channel = await _connection.CreateChannelAsync();

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _channel.BasicPublishAsync(exchange: "", routingKey: "dead_letter_queue", body: body);
        }
    }
}
