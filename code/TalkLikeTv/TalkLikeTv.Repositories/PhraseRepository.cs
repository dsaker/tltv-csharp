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

    public async Task<List<Phrase>> GetPhrasesByTitleIdAsync(int titleId, CancellationToken cancel = default)
    {
        string cacheKey = $"phrases_by_title_{titleId}";
        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ => await _db.Phrases.Where(ph => ph.TitleId == titleId).ToListAsync(cancel),
            cancellationToken: cancel);
    }

    public async Task<Phrase?> RetrieveAsync(string id, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var phraseId))
        {
            return null;
        }

        return await _cache.GetOrCreateAsync(
            id,
            async _ => await _db.Phrases.FirstOrDefaultAsync(p => p.PhraseId == phraseId, cancel),
            cancellationToken: cancel);
    }

    public async Task<Phrase> CreateAsync(Phrase phrase, CancellationToken cancel = default)
    {
        _db.Phrases.Add(phrase);
        await _db.SaveChangesAsync(cancel);

        // Add to cache after successful save
        await _cache.SetAsync(phrase.PhraseId.ToString(), phrase, cancellationToken: cancel);

        return phrase;
    }

    public async Task<bool> UpdateAsync(string id, Phrase phrase, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var phraseId))
        {
            return false;
        }

        var existingPhrase = await _db.Phrases.FirstOrDefaultAsync(p => p.PhraseId == phraseId, cancel);
        if (existingPhrase == null)
        {
            return false;
        }

        _db.Entry(existingPhrase).CurrentValues.SetValues(phrase);
        await _db.SaveChangesAsync(cancel);

        // Update cache
        await _cache.SetAsync(id, phrase, cancellationToken: cancel);

        // Invalidate title phrases cache
        var titleId = phrase.TitleId.GetValueOrDefault(); // Handle nullable TitleId
        await _cache.RemoveAsync($"phrases_by_title_{titleId}", cancel);

        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var phraseId))
        {
            return false;
        }

        var phrase = await _db.Phrases.FirstOrDefaultAsync(p => p.PhraseId == phraseId, cancel);
        if (phrase == null)
        {
            return false;
        }

        var titleId = phrase.TitleId.GetValueOrDefault(); // Handle nullable TitleId
        _db.Phrases.Remove(phrase);
        await _db.SaveChangesAsync(cancel);

        // Remove from cache
        await _cache.RemoveAsync(id, cancel);

        // Invalidate title phrases cache
        await _cache.RemoveAsync($"phrases_by_title_{titleId}", cancel);

        return true;
    }
    
    public async Task<List<Phrase>> CreateManyAsync(List<Phrase> phrases, CancellationToken cancel = default)
    {
        _db.Phrases.AddRange(phrases);
        await _db.SaveChangesAsync(cancel);
    
        // Invalidate relevant cache entries
        foreach (var phrase in phrases)
        {
            // Cache the individual phrase
            await _cache.SetAsync(phrase.PhraseId.ToString(), phrase, cancellationToken: cancel);
        }
    
        return phrases;
    }
}