using System.Security.Cryptography;
using System.Text;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Exceptions;

namespace TalkLikeTv.Services;

public class TokenService
{
    private readonly ITokenRepository _tokenRepository;

    public TokenService(ITokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }
    
    public async Task<bool> CheckTokenStatus(string token)
    {
        token = token ?? throw new ArgumentNullException(nameof(token));

        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var hashString = Convert.ToHexStringLower(tokenHash);

        var dbToken = await _tokenRepository.RetrieveByHashAsync(hashString);
        if (dbToken == null)
        {
            throw new TokenNotFoundException();
        }
        
        if (dbToken.Used)
        {
            throw new InvalidOperationException("Token already used.");
        }
        
        return true;
    }
}