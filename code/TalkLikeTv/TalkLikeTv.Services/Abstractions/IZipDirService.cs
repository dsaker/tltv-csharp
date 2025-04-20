namespace TalkLikeTv.Services.Abstractions;

public interface IZipDirService
{
    FileInfo ZipStringsList(List<string> stringList, int max, string txtPath, string filename);
    FileInfo CreateZipFile(string sourceDir, string filename);
}