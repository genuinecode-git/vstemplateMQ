
namespace TemplateMQ.Domain.Models;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; } = 0;
}
