using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TalkLikeTv.EntityModels;

namespace TalkLikeTv.Services;

public class AzureTextToSpeechService
{
    private readonly SpeechConfig _speechConfig;

    public AzureTextToSpeechService()
    {
        var subscriptionKey = Environment.GetEnvironmentVariable("AZURE_TTS_KEY");
        var region = Environment.GetEnvironmentVariable("AZURE_REGION");

        if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
        {
            throw new InvalidOperationException("Azure subscription key and region must be set in environment variables.");
        }

        _speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
    }

    public async Task GenerateSpeechToFileAsync(string text, Voice voice, string filePath)
    {
        var audioConfig = AudioConfig.FromWavFileOutput(filePath);
        var synthesizer = new SpeechSynthesizer(_speechConfig, audioConfig);

        var voiceName = $"{voice.LocaleName}-{voice.ShortName}";
        _speechConfig.SpeechSynthesisVoiceName = voiceName;

        var result = await synthesizer.SpeakTextAsync(text);

        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new Exception($"Speech synthesis failed: {result.Reason}");
        }
    }
}