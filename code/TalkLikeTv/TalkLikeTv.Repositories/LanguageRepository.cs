using TalkLikeTv.EntityModels; // To use Language.
using Microsoft.EntityFrameworkCore; // To use ToArrayAsync.
using Microsoft.Extensions.Caching.Hybrid; // To use HybridCache.

namespace TalkLikeTv.Repositories;

public class LanguageRepository : ILanguageRepository
{
  private readonly HybridCache _cache;

  // Use an instance data context field because it should not be
  // cached due to the data context having internal caching.
  private TalkliketvContext _db;

  public LanguageRepository(TalkliketvContext db,
    HybridCache hybridCache)
  {
    _db = db;
    _cache = hybridCache;
  }

  public Task<Language[]> RetrieveAllAsync()
  {
    return _db.Languages.OrderBy(l => l.Name).ThenBy(l => l.NativeName).ToArrayAsync();
  }

  public async Task<Language?> RetrieveAsync(string id, CancellationToken token = default)
  {
    return await _cache.GetOrCreateAsync(
      id,
      async _ => await _db.Languages.FirstOrDefaultAsync(l => l.LanguageId.ToString() == id, token),
      cancellationToken: token);
  }
  
  public async Task<Language?> RetrieveByTagAsync(string tag, CancellationToken token = default)
  {
    var cacheKey = $"language_code_{tag}";
    
    return await _cache.GetOrCreateAsync(
      cacheKey,
      async _ => await _db.Languages.FirstOrDefaultAsync(l => l.Tag == tag, token),
      cancellationToken: token);
  }
}
