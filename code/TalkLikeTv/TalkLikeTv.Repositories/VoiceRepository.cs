using TalkLikeTv.EntityModels; // To use Voice.
using Microsoft.EntityFrameworkCore; // To use ToArrayAsync.
using Microsoft.Extensions.Caching.Hybrid; // To use HybridCache.

namespace TalkLikeTv.Repositories;

public class VoiceRepository : IVoiceRepository
{
    private readonly HybridCache _cache;

    // Use an instance data context field because it should not be
    // cached due to the data context having internal caching.
    private TalkliketvContext _db;

    public VoiceRepository(TalkliketvContext db,
        HybridCache hybridCache)
    {
        _db = db;
        _cache = hybridCache;
    }

    public Task<Voice[]> RetrieveAllAsync()
    {
        return _db.Voices
            .Include(v => v.Styles)
            .Include(v => v.Scenarios)
            .Include(v => v.Personalities)
            .OrderBy(v => v.DisplayName)
            .ToArrayAsync();;
    }

    public async Task<Voice?> RetrieveAsync(string id, CancellationToken token = default)
    {
        if (!int.TryParse(id, out var voiceId))
        {
            return null;
        }
        
        return await _cache.GetOrCreateAsync(
            id,
            async _ => await _db.Voices.FirstOrDefaultAsync(v => v.VoiceId == voiceId, token),
            cancellationToken: token);
    }
}