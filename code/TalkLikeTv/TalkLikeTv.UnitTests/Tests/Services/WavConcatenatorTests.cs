using NAudio.Wave;
using TalkLikeTv.Services;

namespace TalkLikeTv.UnitTests.Tests.Services;

public class WavConcatenatorTests : IDisposable
{
    private const string InputFile1 = "3SecondsOfSilence.wav";
    private const string InputFile2 = "4SecondsOfSilence.wav";
    private const string OutputFile = "/tmp/test/WavConcatenatorTests/output.wav";
    private const string EmptyInputOutputFile = "/tmp/test/WavConcatenatorTests/EmptyInput.wav";
    
    public void Dispose()
    {
        // Delete the output files after the tests are run
        if (File.Exists(OutputFile))
        {
            File.Delete(OutputFile);
        }

        if (File.Exists(EmptyInputOutputFile))
        {
            File.Delete(EmptyInputOutputFile);
        }
    }

    public WavConcatenatorTests()
    {
        // Ensure the test directory exists
        Directory.CreateDirectory("/tmp/test/WavConcatenatorTests/");
        
        // Ensure the test files are copied to the output directory
        File.Copy($"../../../../TalkLikeTv.Services/Resources/pause/{InputFile1}", InputFile1, true);
        File.Copy($"../../../../TalkLikeTv.Services/Resources/pause/{InputFile2}", InputFile2, true);
        
    }

    [Fact]
    public void ConcatenateWavFiles_CreatesOutputFile()
    {
        var inputFiles = new List<string> { InputFile1, InputFile2 };

        WavConcatenator.ConcatenateWavFiles(inputFiles, OutputFile);

        Assert.True(File.Exists(OutputFile));
    }

    [Fact]
    public void ConcatenateWavFiles_ThrowsException_WhenInputFileDoesNotExist()
    {
        var inputFiles = new List<string> { "nonexistent.wav" };

        Assert.Throws<FileNotFoundException>(() => WavConcatenator.ConcatenateWavFiles(inputFiles, OutputFile));
    }

    [Fact]
    public void ConcatenateWavFiles_ProducesCorrectFormat()
    {
        var inputFiles = new List<string> { InputFile1, InputFile2 };

        WavConcatenator.ConcatenateWavFiles(inputFiles, OutputFile);

        using (var reader = new WaveFileReader(OutputFile))
        {
            Assert.Equal(16000, reader.WaveFormat.SampleRate);
            Assert.Equal(16, reader.WaveFormat.BitsPerSample);
            Assert.Equal(1, reader.WaveFormat.Channels);
        }
    }

    [Fact]
    public void ConcatenateWavFiles_HandlesEmptyInputList()
    {
        var inputFiles = new List<string>();

        WavConcatenator.ConcatenateWavFiles(inputFiles, EmptyInputOutputFile);

        Assert.False(File.Exists(EmptyInputOutputFile));
    }
}