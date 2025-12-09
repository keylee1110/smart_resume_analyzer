namespace TheChatbot.Models;

public class ChatContext
{
    public string CvText { get; set; } = string.Empty;
    public string? JobDescription { get; set; }
    public AnalysisResult? LastAnalysis { get; set; } // The full analysis result
    public string UserMessage { get; set; } = string.Empty;
    public List<ChatMessage> ChatHistory { get; set; } = new();
}
