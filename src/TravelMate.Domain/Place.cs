namespace TravelMate.Domain;

public sealed record Place(
    Guid Id,
    string Name,
    string Country,
    string Region,
    GeoPoint Location,
    IReadOnlyCollection<string> Categories);
