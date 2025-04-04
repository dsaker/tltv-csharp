using TalkLikeTv.EntityModels; // To use Translate.
using Microsoft.EntityFrameworkCore; // To use ToArrayAsync.
using Microsoft.Extensions.Caching.Hybrid; // To use HybridCache.

namespace TalkLikeTv.Repositories;

public class TranslateRepository : ITranslateRepository
{
    private readonly HybridCache _cache;

    // Use an instance data context field because it should not be
    // cached due to the data context having internal caching.
    private TalkliketvContext _db;

    public TranslateRepository(TalkliketvContext db,
        HybridCache hybridCache)
    {
        _db = db;
        _cache = hybridCache;
    }

    public Task<Translate[]> RetrieveAllAsync(CancellationToken token = default)
    {
        return _db.Translates.ToArrayAsync(token);
    }

    public async Task<Translate?> RetrieveAsync(string phraseId, string languageId, CancellationToken token = default)
    {
        if (!int.TryParse(phraseId, out var phraseIdInt) || !int.TryParse(languageId, out var languageIdInt))
        {
            return null;
        }

        var cacheKey = $"translate_{phraseIdInt}_{languageIdInt}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async _ => await _db.Translates
                .Include(t => t.Language)
                .Include(t => t.PhraseNavigation)
                .FirstOrDefaultAsync(t => t.PhraseId == phraseIdInt && t.LanguageId == languageIdInt, token),
            cancellationToken: token);
    }

    public async Task<Translate> CreateAsync(Translate translate, CancellationToken token = default)
    {
        _db.Translates.Add(translate);
        await _db.SaveChangesAsync(token);

        // Create composite cache key
        var cacheKey = $"translate_{translate.PhraseId}_{translate.LanguageId}";
        await _cache.SetAsync(cacheKey, translate, cancellationToken: token);

        return translate;
    }

    public async Task<bool> UpdateAsync(string phraseId, string languageId, Translate translate, CancellationToken token = default)
    {
        if (!int.TryParse(phraseId, out var phraseIdInt) || !int.TryParse(languageId, out var languageIdInt))
        {
            return false;
        }

        var existingTranslate = await _db.Translates.FirstOrDefaultAsync(
            t => t.PhraseId == phraseIdInt && t.LanguageId == languageIdInt, token);

        if (existingTranslate == null)
        {
            return false;
        }

        _db.Entry(existingTranslate).CurrentValues.SetValues(translate);
        await _db.SaveChangesAsync(token);

        // Update cache with composite key
        var cacheKey = $"translate_{phraseIdInt}_{languageIdInt}";
        await _cache.SetAsync(cacheKey, translate, cancellationToken: token);

        return true;
    }

    public async Task<bool> DeleteAsync(string phraseId, string languageId, CancellationToken token = default)
    {
        if (!int.TryParse(phraseId, out var phraseIdInt) || !int.TryParse(languageId, out var languageIdInt))
        {
            return false;
        }

        var translate = await _db.Translates.FirstOrDefaultAsync(
            t => t.PhraseId == phraseIdInt && t.LanguageId == languageIdInt, token);

        if (translate == null)
        {
            return false;
        }

        _db.Translates.Remove(translate);
        await _db.SaveChangesAsync(token);

        // Remove from cache using composite key
        var cacheKey = $"translate_{phraseIdInt}_{languageIdInt}";
        await _cache.RemoveAsync(cacheKey, token);

        return true;
    }
    
    public async Task<List<Translate>> CreateManyAsync(List<Translate> translates, CancellationToken token = default)
    {
        _db.Translates.AddRange(translates);
        await _db.SaveChangesAsync(token);

        // Cache each individual translate using composite keys
        foreach (var translate in translates)
        {
            var cacheKey = $"translate_{translate.PhraseId}_{translate.LanguageId}";
            await _cache.SetAsync(cacheKey, translate, cancellationToken: token);
        }

        return translates;
    }
}