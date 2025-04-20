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
    
    public async Task<List<Translate>> CreateManyAsync(List<Translate> translates, CancellationToken cancel = default)
    {
        _db.Translates.AddRange(translates);
        await _db.SaveChangesAsync(cancel);

        // Cache each individual translate using composite keys
        foreach (var translate in translates)
        {
            var cacheKey = $"translate_{translate.PhraseId}_{translate.LanguageId}";
            await _cache.SetAsync(cacheKey, translate, cancellationToken: cancel);
        }

        return translates;
    }
    
    public Task<List<Translate>> GetTranslatesByLanguageAndPhrasesAsync(int languageId, IEnumerable<int> phraseIds, CancellationToken cancel = default)
    {
        return _db.Translates
            .Where(t => t.LanguageId == languageId && phraseIds.Contains(t.PhraseId))
            .ToListAsync(cancel);
    }
}