using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface ITokenRepository
{
    Task<bool> UpdateAsync(string id, Token phrase, CancellationToken cancellationToken = default);
    Task<Token?> RetrieveByHashAsync(string hash, CancellationToken cancellationToken = default);
}