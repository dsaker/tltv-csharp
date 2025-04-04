using TalkLikeTv.EntityModels; // To use Title.
using Microsoft.EntityFrameworkCore; // To use ToArrayAsync.
using Microsoft.Extensions.Caching.Hybrid; // To use HybridCache.

namespace TalkLikeTv.Repositories;

public class TitleRepository : ITitleRepository
{
  private readonly HybridCache _cache;

  // Use an instance data context field because it should not be
  // cached due to the data context having internal caching.
  private TalkliketvContext _db;

  public TitleRepository(TalkliketvContext db,
    HybridCache hybridCache)
  {
    _db = db;
    _cache = hybridCache;
  }

  public Task<Title[]> RetrieveAllAsync(CancellationToken token = default)
  {
    return _db.Titles.Include(t => t.OriginalLanguage).ToArrayAsync(token);
  }

  public async Task<Title?> RetrieveAsync(string id, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var titleId))
        {
            return null;
        }

        return await _cache.GetOrCreateAsync(
            id,
            async _ => await _db.Titles.FirstOrDefaultAsync(p => p.TitleId == titleId, token),
            cancellationToken: token);
    }

    public async Task<Title> CreateAsync(Title title, CancellationToken token = default)
    {
        _db.Titles.Add(title);
        await _db.SaveChangesAsync(token);

        // Add to cache after successful save
        await _cache.SetAsync(title.TitleId.ToString(), title, cancellationToken: token);

        return title;
    }

    public async Task<bool> UpdateAsync(string id, Title title, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var titleId))
        {
            return false;
        }

        var existingTitle = await _db.Titles.FirstOrDefaultAsync(p => p.TitleId == titleId, token);
        if (existingTitle == null)
        {
            return false;
        }

        _db.Entry(existingTitle).CurrentValues.SetValues(title);
        await _db.SaveChangesAsync(token);

        // Update cache
        await _cache.SetAsync(id, title, cancellationToken: token);

        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var titleId))
        {
            return false;
        }

        var title = await _db.Titles.FirstOrDefaultAsync(p => p.TitleId == titleId, token);
        if (title == null)
        {
            return false;
        }

        _db.Titles.Remove(title);
        await _db.SaveChangesAsync(token);

        // Remove from cache
        await _cache.RemoveAsync(id, token);

        return true;
    }
  
  public async Task<Title?> RetrieveByNameAsync(string name,
    CancellationToken token = default)
  {
    return await _cache.GetOrCreateAsync(
      key: name, // Unique key to the cache entry.
      factory: async cancel => await _db.Titles
        .FirstOrDefaultAsync(t => t.TitleName == name, token),
      cancellationToken: token);
  }
  
  public async Task<(Title[] titles, int totalCount)> SearchTitlesAsync(
      int? languageId,
      string? keyword,
      string searchType,
      int pageNumber,
      int pageSize,
      CancellationToken token = default)
  {
      IQueryable<Title> query = _db.Titles.Include(t => t.OriginalLanguage);

      switch (searchType)
      {
          case "Language" when languageId.HasValue:
              query = query.Where(t => t.OriginalLanguageId == languageId);
              break;
            
          case "Keyword" when !string.IsNullOrEmpty(keyword):
              query = query.Where(t => (t.TitleName).Contains(keyword) ||
                                       (t.Description ?? "").Contains(keyword));
              break;
            
          case "Both" when languageId.HasValue && !string.IsNullOrEmpty(keyword):
              query = query.Where(t => t.OriginalLanguageId == languageId &&
                                       ((t.TitleName).Contains(keyword) ||
                                        (t.Description ?? "").Contains(keyword)));
              break;
      }

      var totalCount = await query.CountAsync(token);

      var titles = await query
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .OrderBy(t => t.Popularity)
          .ToArrayAsync(token);

      return (titles, totalCount);
  }
}
