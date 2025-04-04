using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public class PhraseRepository : IPhraseRepository
{
    private readonly TalkliketvContext _db;
    private readonly HybridCache _cache;

    public PhraseRepository(TalkliketvContext db, HybridCache hybridCache)
    {
        _db = db;
        _cache = hybridCache;
    }

    public async Task<List<Phrase>> GetPhrasesByTitleIdAsync(int titleId, CancellationToken token = default)
    {
        string cacheKey = $"phrases_by_title_{titleId}";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ => await _db.Phrases.Where(ph => ph.TitleId == titleId).ToListAsync(token),
            cancellationToken: token);
    }

    public async Task<Phrase?> RetrieveAsync(string id, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var phraseId))
        {
            return null;
        }

        return await _cache.GetOrCreateAsync(
            id,
            async _ => await _db.Phrases.FirstOrDefaultAsync(p => p.PhraseId == phraseId, token),
            cancellationToken: token);
    }

    public async Task<Phrase> CreateAsync(Phrase phrase, CancellationToken token = default)
    {
        _db.Phrases.Add(phrase);
        await _db.SaveChangesAsync(token);

        // Add to cache after successful save
        await _cache.SetAsync(phrase.PhraseId.ToString(), phrase, cancellationToken: token);

        return phrase;
    }

    public async Task<bool> UpdateAsync(string id, Phrase phrase, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var phraseId))
        {
            return false;
        }

        var existingPhrase = await _db.Phrases.FirstOrDefaultAsync(p => p.PhraseId == phraseId, token);
        if (existingPhrase == null)
        {
            return false;
        }

        _db.Entry(existingPhrase).CurrentValues.SetValues(phrase);
        await _db.SaveChangesAsync(token);

        // Update cache
        await _cache.SetAsync(id, phrase, cancellationToken: token);

        // Invalidate title phrases cache
        var titleId = phrase.TitleId.GetValueOrDefault(); // Handle nullable TitleId
        await _cache.RemoveAsync($"phrases_by_title_{titleId}", token);

        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var phraseId))
        {
            return false;
        }

        var phrase = await _db.Phrases.FirstOrDefaultAsync(p => p.PhraseId == phraseId, token);
        if (phrase == null)
        {
            return false;
        }

        var titleId = phrase.TitleId.GetValueOrDefault(); // Handle nullable TitleId
        _db.Phrases.Remove(phrase);
        await _db.SaveChangesAsync(token);

        // Remove from cache
        await _cache.RemoveAsync(id, token);

        // Invalidate title phrases cache
        await _cache.RemoveAsync($"phrases_by_title_{titleId}", token);

        return true;
    }
    
    public async Task<List<Phrase>> CreateManyAsync(List<Phrase> phrases, CancellationToken token = default)
    {
        _db.Phrases.AddRange(phrases);
        await _db.SaveChangesAsync(token);
    
        // Invalidate relevant cache entries
        foreach (var phrase in phrases)
        {
            // Cache the individual phrase
            await _cache.SetAsync(phrase.PhraseId.ToString(), phrase, cancellationToken: token);
        }
    
        return phrases;
    }
}