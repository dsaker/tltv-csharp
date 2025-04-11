using System.Security.Cryptography;
using System.Text;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Exceptions;

namespace TalkLikeTv.Services;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;

    public TokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public async Task<ITokenService.TokenResult> CheckTokenStatus(string token, CancellationToken cancellationToken = default)
    {
        token = token ?? throw new ArgumentNullException(nameof(token));

        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var hashString = Convert.ToHexStringLower(tokenHash);

        var dbToken = await _tokenRepository.RetrieveByHashAsync(hashString, cancellationToken);
        if (dbToken == null)
        {
            return new ITokenService.TokenResult { Success = false, ErrorMessage = "Token not found." };
        }

        if (dbToken.Used)
        {
            return new ITokenService.TokenResult { Success = false, ErrorMessage = "Token already used." };
        }

        return new ITokenService.TokenResult { Success = true };
    }
}