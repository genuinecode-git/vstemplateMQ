namespace TemplateMQ.API.Application.Models.ConfigurationModels;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "transaction_queue";
    public string DeadLetterQueue { get; set; } = "dead_letter_queue";
}

