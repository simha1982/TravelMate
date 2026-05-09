using System.Net.Http.Json;
using System.Text;

namespace TravelMate.AI;

public interface ITextToSpeechGateway
{
    Task<SpeechAudio> SynthesizeAsync(
        string text,
        string languageCode,
        string voiceName,
        CancellationToken cancellationToken);
}

public sealed record SpeechAudio(byte[] Content, string ContentType, string FileExtension);

public sealed class StubTextToSpeechGateway : ITextToSpeechGateway
{
    public Task<SpeechAudio> SynthesizeAsync(
        string text,
        string languageCode,
        string voiceName,
        CancellationToken cancellationToken)
    {
        var content = Encoding.UTF8.GetBytes($"TTS stub for {languageCode}/{voiceName}: {text}");
        return Task.FromResult(new SpeechAudio(content, "text/plain", "txt"));
    }
}

public sealed class AzureSpeechOptions
{
    public string Region { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultVoiceName { get; set; } = "en-US-JennyNeural";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Region)
        && !string.IsNullOrWhiteSpace(ApiKey);
}

public sealed class AzureSpeechGateway(HttpClient httpClient, AzureSpeechOptions options) : ITextToSpeechGateway
{
    public async Task<SpeechAudio> SynthesizeAsync(
        string text,
        string languageCode,
        string voiceName,
        CancellationToken cancellationToken)
    {
        if (!options.IsConfigured)
        {
            throw new InvalidOperationException("Azure Speech is not configured. Set AzureSpeech:Region and AzureSpeech:ApiKey.");
        }

        var selectedVoice = string.IsNullOrWhiteSpace(voiceName) ? options.DefaultVoiceName : voiceName;
        var ssml = $"""
            <speak version='1.0' xml:lang='{XmlEscape(languageCode)}'>
              <voice name='{XmlEscape(selectedVoice)}'>{System.Security.SecurityElement.Escape(text)}</voice>
            </speak>
            """;

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://{options.Region}.tts.speech.microsoft.com/cognitiveservices/v1");
        request.Headers.Add("Ocp-Apim-Subscription-Key", options.ApiKey);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
        request.Headers.Add("User-Agent", "TravelMate");
        request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return new SpeechAudio(await response.Content.ReadAsByteArrayAsync(cancellationToken), "audio/mpeg", "mp3");
    }

    private static string XmlEscape(string value) =>
        System.Security.SecurityElement.Escape(value) ?? string.Empty;
}
