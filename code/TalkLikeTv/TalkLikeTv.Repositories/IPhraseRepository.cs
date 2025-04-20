using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface IPhraseRepository
{
    Task<List<Phrase>> GetPhrasesByTitleIdAsync(int titleId, CancellationToken cancel = default);
    Task<List<Phrase>> CreateManyAsync(List<Phrase> phrases, CancellationToken token = default);
}