namespace TalkLikeTv.Services.Abstractions;

public interface ITokenService
{
    public class TokenResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    Task<TokenResult> CheckTokenStatus(string? tokenHash, CancellationToken token = default);
}