using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services
{
    public interface ITitleValidationService
    {
        Task<(bool IsValid, IEnumerable<string> Errors)> ValidateAsync(Title title, CancellationToken token = default);
    }
}