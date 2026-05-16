namespace SmartApiary.Infrastructure.Helpers;

public static class SpatialHelper
{
    public const double DefaultRadiusKm = 5.0;

    public static double HaversineDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    public static bool IsWithinRadiusKm(double lat1, double lon1, double lat2, double lon2, double radiusKm) =>
        HaversineDistanceKm(lat1, lon1, lat2, lon2) <= radiusKm;

    private static double ToRad(double deg) => deg * Math.PI / 180;
}
