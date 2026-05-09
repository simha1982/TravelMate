namespace TravelMate.Application;

public sealed record StorySummaryRequest(
    string PlaceName,
    string SourceText,
    string LanguageCode = "en",
    int MaxWords = 120);

public sealed record StorySummaryResponse(
    string Title,
    string Summary,
    string[] Categories,
    string[] SafetyNotes);

public sealed record SpeechSynthesisRequest(
    string Text,
    string LanguageCode = "en-US",
    string VoiceName = "en-US-JennyNeural");
