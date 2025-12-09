using TheChatbot.Models;
using System.Threading;
using System.Threading.Tasks;

namespace TheChatbot.Services;

public interface IBedrockChatService
{
    Task<string> GetChatResponseAsync(ChatContext context, CancellationToken cancellationToken = default);
}
