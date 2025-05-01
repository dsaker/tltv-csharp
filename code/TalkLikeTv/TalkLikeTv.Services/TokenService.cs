using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.Services;

public class TokenService : ITokenService
{
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<TokenService> _logger;

    public TokenService(ITokenRepository tokenRepository, ILogger<TokenService> logger)
    {
        _tokenRepository = tokenRepository;
        _logger = logger;
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

        return new ITokenService.TokenResult { Token = dbToken, Success = true };
    }
    
    public async Task<(bool Success, List<string> Errors)> MarkTokenAsUsedAsync(Token token, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
    
        try
        {
            token.Used = true;
            await _tokenRepository.UpdateAsync(token.TokenId.ToString(), token, cancellationToken);
            return (true, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking token {TokenHash} as used", token.Hash);
            errors.Add("An error occurred while processing the token.");
            return (false, errors);
        }
    }
    
    public (Token token, string plaintext) GenerateToken()
    {
        // Generate 16 random bytes
        var randomBytes = new byte[16];
        RandomNumberGenerator.Fill(randomBytes);

        // Encode the random bytes to a Base32 string without padding
        var plaintext = Base32Encode(randomBytes);

        // Compute the SHA-256 hash of the plaintext
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));
        var hashString = Convert.ToHexString(hash).ToLowerInvariant();

        // Create the Token object
        var token = new Token
        {
            Hash = hashString,
            Created = DateTime.UtcNow
        };

        return (token, plaintext);
    }

    private string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var output = new StringBuilder();
        int buffer = data[0];
        var bitsLeft = 8;
        var index = 1;

        while (bitsLeft > 0 || index < data.Length)
        {
            if (bitsLeft < 5)
            {
                if (index < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[index++] & 0xFF;
                    bitsLeft += 8;
                }
                else
                {
                    var pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }

            var mask = 0b11111 << (bitsLeft - 5);
            var value = (buffer & mask) >> (bitsLeft - 5);
            bitsLeft -= 5;
            output.Append(alphabet[value]);
        }

        return output.ToString();
    }
}