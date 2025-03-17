using NAudio.Wave;

namespace TalkLikeTv.Services;

public class WavConcatenator
{
    public static void ConcatenateWavFiles(IEnumerable<string> inputFilePaths, string outputFilePath)
    {
        using (var outputStream = new WaveFileWriter(outputFilePath, new WaveFormat()))
        {
            foreach (var inputFilePath in inputFilePaths)
            {
                using (var inputReader = new WaveFileReader(inputFilePath))
                {
                    if (outputStream.WaveFormat.SampleRate != inputReader.WaveFormat.SampleRate ||
                        outputStream.WaveFormat.Channels != inputReader.WaveFormat.Channels)
                    {
                        throw new InvalidOperationException("All WAV files must have the same format.");
                    }

                    var buffer = new byte[inputReader.Length];
                    int bytesRead;
                    while ((bytesRead = inputReader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}