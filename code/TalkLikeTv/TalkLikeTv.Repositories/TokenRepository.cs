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

    public async Task<Token?> RetrieveAsync(string id, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var tokenId))
        {
            return null;
        }

        return await _cache.GetOrCreateAsync(
            id,
            async _ => await _db.Tokens.FirstOrDefaultAsync(p => p.TokenId == tokenId, token),
            cancellationToken: token);
    }

    public async Task<Token> CreateAsync(Token token, CancellationToken cancel = default)
    {
        _db.Tokens.Add(token);
        await _db.SaveChangesAsync(cancel);

        // Add to cache after successful save
        await _cache.SetAsync(token.TokenId.ToString(), token, cancellationToken: cancel);

        return token;
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

    public async Task<bool> DeleteAsync(string id, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var tokenId))
        {
            return false;
        }

        var token = await _db.Tokens.FirstOrDefaultAsync(p => p.TokenId == tokenId, cancel);
        if (token == null)
        {
            return false;
        }

        _db.Tokens.Remove(token);
        await _db.SaveChangesAsync(cancel);

        // Remove from cache
        await _cache.RemoveAsync(id, cancel);

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