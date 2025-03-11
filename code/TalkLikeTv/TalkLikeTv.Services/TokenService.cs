using System.Security.Cryptography;
using System.Text;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Services.Exceptions;

namespace TalkLikeTv.Services;

public class TokenService
{
    private readonly TalkliketvContext _db;

    public TokenService(TalkliketvContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }
    
    public bool CheckTokenStatus(string token)
    {
        token = token ?? throw new ArgumentNullException(nameof(token));

        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var hashString = Convert.ToHexStringLower(tokenHash);

        var dbToken = _db.Tokens.FirstOrDefault(t => t.Hash == hashString);
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