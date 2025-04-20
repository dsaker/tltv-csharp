using TalkLikeTv.EntityModels; // To use Voice.

namespace TalkLikeTv.Repositories;

public interface IVoiceRepository
{
    Task<Voice[]> RetrieveAllAsync(CancellationToken token);
    Task<Voice?> RetrieveAsync(string id, CancellationToken token);
}