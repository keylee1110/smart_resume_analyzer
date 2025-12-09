namespace TheChatbot.Models;

public class ChatRequest
{
    public string ResumeId { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string? JobDescription { get; set; } // Optional: user might provide a new JD or just use existing
    public List<ChatMessage> ChatHistory { get; set; } = new();
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty; // ISO 8601 format
}
