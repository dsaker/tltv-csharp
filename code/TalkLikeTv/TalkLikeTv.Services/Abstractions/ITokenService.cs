using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services.Abstractions;

public interface ITokenService
{
    public class TokenResult
    {
        public Token? Token { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    Task<TokenResult> CheckTokenStatus(string? tokenHash, CancellationToken token = default);
    Task<(bool Success, List<string> Errors)> MarkTokenAsUsedAsync(Token token, CancellationToken cancellationToken = default);

}