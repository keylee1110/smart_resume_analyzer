using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Microsoft.Extensions.Logging;
using TheChatbot.Models;
using System.Text;
using System.Text.Json;

namespace TheChatbot.Services;

public class BedrockChatService : IBedrockChatService
{
    private readonly IAmazonBedrockRuntime _bedrockRuntimeClient;
    private readonly ILogger<BedrockChatService> _logger;
    private readonly string _bedrockModelId;
    private const int MaxContextLength = 100000; // Claude 3 context window (approximate)
    private const int MaxTokens = 2000;

    public BedrockChatService(IAmazonBedrockRuntime bedrockRuntimeClient, ILogger<BedrockChatService> logger)
    {
        _bedrockRuntimeClient = bedrockRuntimeClient;
        _logger = logger;
        // Allow model ID to be configured via environment variable
        _bedrockModelId = Environment.GetEnvironmentVariable("BEDROCK_MODEL_ID") 
            ?? "anthropic.claude-3-sonnet-20240229-v1:0";
    }

    public async Task<string> GetChatResponseAsync(ChatContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating chat response using Bedrock model: {ModelId}", _bedrockModelId);

        try
        {
            // Construct the system prompt with context
            var systemPrompt = BuildSystemPrompt(context);
            
            // Build message history with context window management
            var messages = BuildMessageHistory(context, systemPrompt.Length);
            
            // Prepare the request body for Claude 3
            var body = new
            {
                anthropic_version = "bedrock-2023-05-31",
                max_tokens = MaxTokens,
                system = systemPrompt,
                messages = messages,
                temperature = 0.7
            };

            var request = new InvokeModelRequest
            {
                ModelId = _bedrockModelId,
                ContentType = "application/json",
                Accept = "application/json",
                Body = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body)))
            };

            var response = await _bedrockRuntimeClient.InvokeModelAsync(request, cancellationToken);
            using var responseReader = new StreamReader(response.Body);
            var responseBody = await responseReader.ReadToEndAsync();
            
            _logger.LogDebug("Bedrock response: {Response}", responseBody);
            
            var jsonDoc = JsonDocument.Parse(responseBody);

            // Extract content from Claude 3 Messages API response
            if (jsonDoc.RootElement.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in contentArray.EnumerateArray())
                {
                    if (item.TryGetProperty("text", out var textElement))
                    {
                        var aiResponse = textElement.GetString() ?? "No response from AI.";
                        _logger.LogInformation("Successfully generated AI response");
                        return aiResponse;
                    }
                }
            }
            
            _logger.LogWarning("No text content found in AI response");
            return "No text content found in AI response.";
        }
        catch (AmazonBedrockRuntimeException ex)
        {
            _logger.LogError(ex, "Bedrock Runtime Exception: {Message}", ex.Message);
            // Hint for common error: "AccessDeniedException: Your account is not authorized to invoke this API operation."
            if (ex.Message.Contains("AccessDeniedException") || ex.Message.Contains("unauthorized"))
            {
                 throw new InvalidOperationException($"Access Denied to Bedrock Model '{_bedrockModelId}'. Please ensure you have requested access to 'Claude 3 Sonnet' in the AWS Bedrock Console (Model access). Original Error: {ex.Message}", ex);
            }
            throw new InvalidOperationException($"Error communicating with Bedrock: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error invoking Bedrock: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Builds the system prompt with CV, JD, and analysis context
    /// </summary>
    private string BuildSystemPrompt(ChatContext context)
    {
        var systemPrompt = new StringBuilder();
        
        systemPrompt.AppendLine("You are an Expert Senior Technical Recruiter and Career Coach with 15+ years of experience.");
        systemPrompt.AppendLine("Your goal is to help the candidate improve their resume and interview preparation based on the specific Job Description provided.");
        systemPrompt.AppendLine();
        systemPrompt.AppendLine("=== CONTEXT ===");
        systemPrompt.AppendLine($"CANDIDATE NAME: {context.LastAnalysis?.ResumeEntities?.Name ?? "Candidate"}");
        
        if (context.LastAnalysis != null)
        {
             systemPrompt.AppendLine($"FIT SCORE: {context.LastAnalysis.FitScore:F1}/100");
             
             if (context.LastAnalysis.MissingSkills != null && context.LastAnalysis.MissingSkills.Any())
             {
                 systemPrompt.AppendLine($"MISSING SKILLS: {string.Join(", ", context.LastAnalysis.MissingSkills)}");
             }
        }
        
        systemPrompt.AppendLine();
        systemPrompt.AppendLine("=== INSTRUCTIONS ===");
        systemPrompt.AppendLine("1. **Be Direct & Actionable**: Avoid generic fluff. Give specific feedback.");
        systemPrompt.AppendLine("2. **Focus on the Gap**: prioritizing addressing missing skills and weak areas.");
        systemPrompt.AppendLine("3. **Roleplay**: If asked about interview questions, act as the hiring manager for this specific role.");
        systemPrompt.AppendLine("4. **Format**: Use Markdown (bolding, lists) to make your advice easy to read.");
        systemPrompt.AppendLine("5. **Tone**: Professional, encouraging, but honest and high-standards.");
        systemPrompt.AppendLine();
        
        // Add CV context (truncate if too long)
        systemPrompt.AppendLine("=== CANDIDATE RESUME ===");
        var cvText = TruncateText(context.CvText, 3000);
        systemPrompt.AppendLine(cvText);
        systemPrompt.AppendLine();
        
        // Add Job Description context if available
        if (!string.IsNullOrWhiteSpace(context.JobDescription))
        {
            systemPrompt.AppendLine("=== TARGET JOB DESCRIPTION ===");
            var jdText = TruncateText(context.JobDescription, 2000);
            systemPrompt.AppendLine(jdText);
            systemPrompt.AppendLine();
        }
        
        systemPrompt.AppendLine("Based on this context, answer the user's questions.");
        
        return systemPrompt.ToString();
    }

    /// <summary>
    /// Builds the message history with context window management and role sanitization
    /// </summary>
    private List<object> BuildMessageHistory(ChatContext context, int systemPromptLength)
    {
        var messages = new List<object>();
        var currentLength = systemPromptLength;
        var historyToInclude = new List<ChatMessage>();
        
        // 1. Select history messages that fit in the context window
        if (context.ChatHistory != null && context.ChatHistory.Any())
        {
            // Start from most recent and work backwards
            for (int i = context.ChatHistory.Count - 1; i >= 0; i--)
            {
                var msg = context.ChatHistory[i];
                var msgLength = msg.Content.Length;
                
                // Check if adding this message would exceed context window
                if (currentLength + msgLength > MaxContextLength)
                {
                    _logger.LogInformation("Truncating chat history to fit context window");
                    break;
                }
                
                historyToInclude.Insert(0, msg); // Insert at beginning to maintain chronological order
                currentLength += msgLength;
            }
        }
        
        // 2. Intelligently append the current UserMessage
        if (historyToInclude.Count > 0)
        {
            var lastMsg = historyToInclude.Last();
            
            if (string.Equals(lastMsg.Role, "user", StringComparison.OrdinalIgnoreCase))
            {
                if (lastMsg.Content.Trim() == context.UserMessage.Trim())
                {
                    // Case 3a: Duplicate message (frontend likely sent updated history). Do not append.
                    _logger.LogInformation("Detected duplicate user message at end of history. Skipping append.");
                }
                else
                {
                    // Case 3b: Consecutive user messages. Merge them to satisfy "User -> Assistant" alternation.
                    _logger.LogInformation("Merging consecutive user messages.");
                    
                    // Create a new merged message object to avoid modifying the original reference
                    var mergedMsg = new ChatMessage 
                    { 
                        Role = "user", 
                        Content = lastMsg.Content + "\n\n" + context.UserMessage,
                        Timestamp = lastMsg.Timestamp 
                    };
                    
                    // Replace the last message with the merged one
                    historyToInclude.RemoveAt(historyToInclude.Count - 1);
                    historyToInclude.Add(mergedMsg);
                }
            }
            else
            {
                // Case 4: Last was assistant. Append new user message.
                historyToInclude.Add(new ChatMessage { Role = "user", Content = context.UserMessage });
            }
        }
        else
        {
            // History is empty. Just add the user message.
            historyToInclude.Add(new ChatMessage { Role = "user", Content = context.UserMessage });
        }
        
        // 3. Convert to Bedrock API format
        foreach (var msg in historyToInclude)
        {
            messages.Add(new { role = msg.Role, content = msg.Content });
        }
        
        return messages;
    }

    /// <summary>
    /// Truncates text to a maximum length while preserving word boundaries
    /// </summary>
    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
        {
            return text;
        }
        
        // Find the last space before maxLength
        var truncated = text.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        
        if (lastSpace > 0)
        {
            truncated = truncated.Substring(0, lastSpace);
        }
        
        return truncated + "...";
    }
}
