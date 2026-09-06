// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace Canopy.Providers;

public static class ClearSkyModel
{
    public static double SolarElevationDegrees(double latDeg, double lonDeg, DateTimeOffset localTime)
    {
        var utc = localTime.ToUniversalTime();
        double dayOfYear = utc.DayOfYear;

        double gamma = 2.0 * Math.PI / 365.0 * (dayOfYear - 1 + (utc.Hour - 12.0) / 24.0);

        double eqTime = 229.18 * (0.000075
            + 0.001868 * Math.Cos(gamma) - 0.032077 * Math.Sin(gamma)
            - 0.014615 * Math.Cos(2 * gamma) - 0.040849 * Math.Sin(2 * gamma));

        double decl = 0.006918
            - 0.399912 * Math.Cos(gamma) + 0.070257 * Math.Sin(gamma)
            - 0.006758 * Math.Cos(2 * gamma) + 0.000907 * Math.Sin(2 * gamma)
            - 0.002697 * Math.Cos(3 * gamma) + 0.00148 * Math.Sin(3 * gamma);

        double timeOffset = eqTime + 4 * lonDeg;
        double trueSolarTimeMin = utc.Hour * 60 + utc.Minute + utc.Second / 60.0 + timeOffset;

        double hourAngleDeg = (trueSolarTimeMin / 4.0) - 180.0;
        double hourAngleRad = hourAngleDeg * Math.PI / 180.0;

        double latRad = latDeg * Math.PI / 180.0;

        double sinElevation =
            Math.Sin(latRad) * Math.Sin(decl) +
            Math.Cos(latRad) * Math.Cos(decl) * Math.Cos(hourAngleRad);

        double elevationRad = Math.Asin(Math.Clamp(sinElevation, -1.0, 1.0));
        return elevationRad * 180.0 / Math.PI;
    }

    // Haurwitz clear-sky global horizontal irradiance estimate (W/m^2).
    public static double EstimateClearSkyRadiation(double elevationDeg)
    {
        if (elevationDeg <= 0)
            return 0.0;

        double zenithRad = (90.0 - elevationDeg) * Math.PI / 180.0;
        double cosZenith = Math.Cos(zenithRad);

        // haurwitz model Ghi = 1098 * cos(z) * exp(-0.057 / cos(z))
        double ghi = 1098.0 * cosZenith * Math.Exp(-0.057 / cosZenith);
        return Math.Max(ghi, 0.0);
    }
}
