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

  public Task<Title[]> RetrieveAllAsync(CancellationToken cancel = default)
  {
    return _db.Titles.Include(t => t.OriginalLanguage).ToArrayAsync(cancel);
  }

  public async Task<Title?> RetrieveAsync(string id, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var titleId))
        {
            return null;
        }

        return await _cache.GetOrCreateAsync(
            id,
            async _ => await _db.Titles.FirstOrDefaultAsync(p => p.TitleId == titleId, cancel),
            cancellationToken: cancel);
    }

    public async Task<Title> CreateAsync(Title title, CancellationToken cancel = default)
    {
        _db.Titles.Add(title);
        await _db.SaveChangesAsync(cancel);

        // Add to cache after successful save
        await _cache.SetAsync(title.TitleId.ToString(), title, cancellationToken: cancel);

        return title;
    }

    public async Task<bool> UpdateAsync(string id, Title title, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var titleId))
        {
            return false;
        }

        var existingTitle = await _db.Titles.FirstOrDefaultAsync(p => p.TitleId == titleId, cancel);
        if (existingTitle == null)
        {
            return false;
        }

        _db.Entry(existingTitle).CurrentValues.SetValues(title);
        await _db.SaveChangesAsync(cancel);

        // Update cache
        await _cache.SetAsync(id, title, cancellationToken: cancel);

        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancel = default)
    {
        if (!int.TryParse(id, out var titleId))
        {
            return false;
        }

        var title = await _db.Titles.FirstOrDefaultAsync(p => p.TitleId == titleId, cancel);
        if (title == null)
        {
            return false;
        }

        _db.Titles.Remove(title);
        await _db.SaveChangesAsync(cancel);

        // Remove from cache
        await _cache.RemoveAsync(id, cancel);

        return true;
    }
  
  public async Task<Title?> RetrieveByNameAsync(string name,
    CancellationToken cancel = default)
  {
    return await _cache.GetOrCreateAsync(
      key: name, // Unique key to the cache entry.
      factory: async cancel => await _db.Titles
        .FirstOrDefaultAsync(t => t.TitleName == name, cancel),
      cancellationToken: cancel);
  }
  
  public async Task<(Title[] titles, int totalCount)> SearchTitlesAsync(
      int? languageId,
      string? keyword,
      string searchType,
      int pageNumber,
      int pageSize,
      CancellationToken cancel = default)
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

      var totalCount = await query.CountAsync(cancel);

      var titles = await query
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .OrderBy(t => t.Popularity)
          .ToArrayAsync(cancel);

      return (titles, totalCount);
  }
}
