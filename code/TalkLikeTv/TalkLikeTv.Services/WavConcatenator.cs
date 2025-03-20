using NAudio.Wave;

namespace TalkLikeTv.Services;

public class WavConcatenator
{
    public static void ConcatenateWavFiles(List<string> inputFiles, string outputFile)
    {
        using (var writer = new WaveFileWriter(outputFile, new WaveFormat(16000, 16, 1))) // Assuming 44.1kHz, 16-bit stereo
        {
            foreach (var file in inputFiles)
            {
                using (var reader = new WaveFileReader(file))
                {
                    byte[] buffer = new byte[reader.WaveFormat.AverageBytesPerSecond];
                    int bytesRead;
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }
}