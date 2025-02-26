namespace TemplateMQ.API.Application.Models
{
    public class ErrorResponse
    {
        public required string Title { get; set; }
        public int Status { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }
}
