namespace TalkLikeTv.Services.Exceptions;

public class TokenNotFoundException : Exception
{
    public TokenNotFoundException() : base("Token not found.") { }

    public TokenNotFoundException(string message) : base(message) { }

    public TokenNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}