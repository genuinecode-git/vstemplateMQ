
namespace TemplateMQ.Domain.Models;

public class InboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } // Event Type
    public string Payload { get; set; } // JSON Serialized Command/Event
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; } // Track retry attempts
    public string? ErrorMessage { get; set; } // Store last error message
}