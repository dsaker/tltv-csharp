using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface ITokenRepository
{
    Task<Token?> RetrieveAsync(string id, CancellationToken token = default);
    Task<Token> CreateAsync(Token phrase, CancellationToken token = default);
    Task<bool> UpdateAsync(string id, Token phrase, CancellationToken token = default);
    Task<bool> DeleteAsync(string id, CancellationToken token = default);
    Task<Token?> RetrieveByHashAsync(string hash, CancellationToken token = default);
}