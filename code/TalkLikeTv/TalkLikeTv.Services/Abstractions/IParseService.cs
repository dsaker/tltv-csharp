namespace TalkLikeTv.Services.Abstractions;

public interface IParseService
{
    ParseService.ParseResult ParseFile(Stream fileStream, string fileName, int maxPhrases);
}