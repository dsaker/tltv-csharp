using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly TalkliketvContext _db;
    private readonly HybridCache _cache;

    public TokenRepository(TalkliketvContext db, HybridCache hybridCache)
    {
        _db = db;
        _cache = hybridCache;
    }
    
    public async Task<bool> UpdateAsync(string id, Token token, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var tokenId))
        {
            return false;
        }

        var existingToken = await _db.Tokens.FirstOrDefaultAsync(p => p.TokenId == tokenId, cancel);
        if (existingToken == null)
        {
            return false;
        }

        _db.Entry(existingToken).CurrentValues.SetValues(token);
        await _db.SaveChangesAsync(cancel);

        // Update cache
        await _cache.SetAsync(id, token, cancellationToken: cancel);

        return true;
    }
    
    public async Task<Token?> RetrieveByHashAsync(string hash, CancellationToken cancel = default)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return null;
        }
    
        var cacheKey = $"token_hash_{hash}";
    
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ => await _db.Tokens.FirstOrDefaultAsync(t => t.Hash == hash, cancellationToken: cancel),
            cancellationToken: cancel);
    }
}