namespace TravelMate.Domain;

public sealed record Story(
    Guid Id,
    Guid PlaceId,
    string Title,
    string ShortDescription,
    string LanguageCode,
    IReadOnlyCollection<string> Categories,
    string SourceName,
    string SourceUrl,
    string? AudioUrl,
    int QualityScore);
