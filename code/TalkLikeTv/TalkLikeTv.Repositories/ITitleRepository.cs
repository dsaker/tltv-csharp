using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Repositories;

public interface ITitleRepository
{
    Task<Title[]> RetrieveAllAsync(CancellationToken token = default);
    Task<Title?> RetrieveAsync(string id, CancellationToken token = default);
    Task<Title> CreateAsync(Title title, CancellationToken token = default);
    Task<bool> UpdateAsync(string id, Title title, CancellationToken token = default);
    Task<bool> DeleteAsync(string id, CancellationToken token = default);
    Task<Title?> RetrieveByNameAsync(string name, CancellationToken token = default);
    Task<(Title[] titles, int totalCount)> SearchTitlesAsync(
        int? languageId,
        string? keyword,
        string searchType,
        int pageNumber,
        int pageSize,
        CancellationToken token = default);
}