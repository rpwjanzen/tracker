using System;

namespace Tracker.Domain;

public readonly record struct YearMonth(int Year, int Month)
{
    public static YearMonth MinValue = new(1, 1);
    
    public static YearMonth FromDateOnly(DateOnly dateOnly) => new (dateOnly.Year, dateOnly.Month);
    public static YearMonth FromDateTime(DateTime value) => new(value.Year, value.Month);
    public DateTime ToDateTime() => new (Year, Month, 1);

    public YearMonth AddMonths(int months) => FromDateOnly(new DateOnly(Year, Month, 1).AddMonths(months));
    public YearMonth AddYears(int years) => this with { Year = Year + years };
    
    public override string ToString() => Year.ToString("D4") + "-" + Month.ToString("D2");

    public static YearMonth Parse(string value)
    {
        var year = int.Parse(value.AsSpan(0, 4));
        var month = int.Parse(value.AsSpan(5, 2));
        return new(year, month);
    }
}

public static class DateOnlyExtensions
{
    public static YearMonth ToYearMonth(this DateOnly date) => YearMonth.FromDateOnly(date);
}
