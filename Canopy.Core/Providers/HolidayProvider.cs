// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Canopy.Configuration;
using Serilog;

namespace Canopy.Providers;

public class HolidayProvider : IProvider<Holiday?>
{
    private static readonly Holiday[] holidays = Enum.GetValues<Holiday>();

    public Holiday? Get()
    {
        var now = DateTimeOffset.Now;
        Holiday? activeHoliday = null;
        foreach (var holiday in holidays)
        {
            if (IsActive(holiday, now.DateTime))
            {
                activeHoliday = holiday;
                break;
            }
        }
#if DEBUG

        Log.Verbose(" ");
        Log.Verbose("Holiday: {h}", activeHoliday);
        Log.Verbose(" ");
#endif

        return activeHoliday;
    }

    public static bool IsActive(Holiday holiday, DateTime targetDate)
    {
        var (start, end) = GetWindow(holiday, targetDate.Year);
        return targetDate >= start && targetDate <= end;
    }

    public static (DateTime Start, DateTime End) GetWindow(Holiday holiday, int year)
    {
        return holiday switch
        {
            //All of December to 27th
            Holiday.Christmas => (
                new DateTime(year, 12, 1),
                new DateTime(year, 12, 27, 23, 59, 59)
            ),

            // New Year: Dec 30 to Jan 3
            Holiday.NewYear => (new DateTime(year - 1, 12, 28), new DateTime(year, 1, 7, 23, 59, 59)),

            // Halloween: Oct 15 to Nov 3
            Holiday.Halloween => (new DateTime(year, 10, 15), new DateTime(year, 11, 3, 23, 59, 59)),

            Holiday.Easter => getEasterWindow(year, daysBefore: 7, daysAfter: 7),

            _ => throw new ArgumentOutOfRangeException(nameof(holiday), holiday, null)
        };
    }

    private static (DateTime Start, DateTime End) getEasterWindow(int year, int daysBefore, int daysAfter)
    {
        DateTime easterSunday = getEasterSunday(year);
        DateTime start = easterSunday.AddDays(-daysBefore);
        DateTime end = easterSunday.AddDays(daysAfter).Add(new TimeSpan(23, 59, 59));
        return (start, end);
    }

    private static DateTime getEasterSunday(int year)
    {
        int metonicCycleIndex = year % 19;

        int century = year / 100;
        int yearInCentury = year % 100;

        int leapCenturies = century / 4;
        int nonLeapCenturies = century % 4;

        int lunarCorrection = (century + 8) / 25;
        int solarCorrection = (century - lunarCorrection + 1) / 3;

        int epact = (19 * metonicCycleIndex + century - leapCenturies - solarCorrection + 15) % 30;

        int leapYearsInCentury = yearInCentury / 4;
        int nonLeapYearsInCentury = yearInCentury % 4;
        int dayOfWeekOffset = (32 + 2 * nonLeapCenturies + 2 * leapYearsInCentury - epact - nonLeapYearsInCentury) % 7;

        int cycleCorrection = (metonicCycleIndex + 11 * epact + 22 * dayOfWeekOffset) / 451;

        int monthIndex = (epact + dayOfWeekOffset - 7 * cycleCorrection + 114) / 31;
        int dayOfMonth = ((epact + dayOfWeekOffset - 7 * cycleCorrection + 114) % 31) + 1;

        return new DateTime(year, monthIndex, dayOfMonth);
    }

    public void Dispose()
    {
    }
}
