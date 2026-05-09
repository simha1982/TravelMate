namespace TravelMate.Domain;

public sealed record GeoPoint(double Latitude, double Longitude)
{
    public double DistanceToMeters(GeoPoint other)
    {
        const double earthRadiusMeters = 6_371_000;
        var lat1 = DegreesToRadians(Latitude);
        var lat2 = DegreesToRadians(other.Latitude);
        var deltaLat = DegreesToRadians(other.Latitude - Latitude);
        var deltaLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
            * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusMeters * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
