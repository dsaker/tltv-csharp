using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TalkLikeTv.EntityModels;
using TalkLikeTv.Services.Abstractions;

namespace TalkLikeTv.Services;

public class AzureTextToSpeechService : IAzureTextToSpeechService
{
    private readonly string?  _subscriptionKey;
    private readonly string? _region;

    public AzureTextToSpeechService()
    {
        _subscriptionKey = Environment.GetEnvironmentVariable("AZURE_TTS_KEY");
        _region = Environment.GetEnvironmentVariable("AZURE_REGION");

        if (string.IsNullOrEmpty(_subscriptionKey) || string.IsNullOrEmpty(_region))
        {
            throw new InvalidOperationException("Azure subscription key and region must be set in environment variables.");
        }
    }

    public async Task GenerateSpeechToFileAsync(string text, Voice voice, string filePath)
    {
        var speechConfig = SpeechConfig.FromSubscription(_subscriptionKey, _region);
        speechConfig.SpeechSynthesisVoiceName = voice.ShortName;

        using var audioConfig = AudioConfig.FromWavFileOutput(filePath);
        using var synthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

        var result = await synthesizer.SpeakTextAsync(text);

        if (result.Reason != ResultReason.SynthesizingAudioCompleted)
        {
            throw new Exception($"Speech synthesis failed: {result.Reason}");
        }
    }
}