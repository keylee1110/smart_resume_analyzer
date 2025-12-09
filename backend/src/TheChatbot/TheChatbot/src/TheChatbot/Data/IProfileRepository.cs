using TheChatbot.Models;
using System.Threading.Tasks;
using System.Threading;

namespace TheChatbot.Data;

public interface IProfileRepository
{
    Task<ProfileRecord?> GetProfileAsync(string resumeId, CancellationToken cancellationToken = default);
}
