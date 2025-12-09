namespace TheChatbot.Data;

using TheChatbot.Models;

public interface IChatHistoryRepository
{
    /// <summary>
    /// Saves a chat message to DynamoDB
    /// </summary>
    /// <param name="resumeId">The resume ID this message belongs to</param>
    /// <param name="message">The chat message to save</param>
    /// <param name="userId">The user ID who owns this conversation</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveMessageAsync(
        string resumeId, 
        ChatMessage message, 
        string userId,
        CancellationToken cancellationToken = default
    );
    
    /// <summary>
    /// Retrieves all chat messages for a specific resume in chronological order
    /// </summary>
    /// <param name="resumeId">The resume ID to get chat history for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of chat messages in chronological order</returns>
    Task<List<ChatMessage>> GetHistoryAsync(
        string resumeId, 
        CancellationToken cancellationToken = default
    );
}
