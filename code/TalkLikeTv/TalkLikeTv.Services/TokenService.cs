using System.Security.Cryptography;
using System.Text;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Repositories;
using TalkLikeTv.Services.Abstractions;

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