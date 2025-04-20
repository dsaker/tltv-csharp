namespace TalkLikeTv.WebApi.Models;

public class ErrorResponse
{
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}