using System;

namespace Tracker.Domain;

public record YearMonth(int Year, int Month) : IComparable<YearMonth>
{
    public static readonly YearMonth MinValue = FromDateOnly(DateOnly.MinValue);
    public static readonly YearMonth MaxValue = FromDateOnly(DateOnly.MaxValue);

    public static YearMonth Now { get { return FromDateTime(DateTime.Now); } }
    public static YearMonth FromDateOnly(DateOnly dateOnly) => new(dateOnly.Year, dateOnly.Month);
    public static YearMonth FromDateTime(DateTime value) => new(value.Year, value.Month);

    public DateTime ToDateTime() => new(Year, Month, 1);
    public DateOnly ToDateOnly() => new(Year, Month, 1);

    public YearMonth AddMonths(int months) => FromDateOnly(new DateOnly(Year, Month, 1).AddMonths(months));
    public YearMonth AddYears(int years) => this with { Year = Year + years };

    public override string ToString() => Year.ToString("D4") + "-" + Month.ToString("D2");

    public static YearMonth Parse(string value)
    {
        var year = int.Parse(value.AsSpan(0, 4));
        var month = int.Parse(value.AsSpan(5, 2));
        return new(year, month);
    }

    public static bool TryParse(string value, out YearMonth yearMonth)
    {
        if (!int.TryParse(value.AsSpan(0, 4), out var year) || !int.TryParse(value.AsSpan(5, 2), out var month))
        {
            yearMonth = MinValue;
            return false;
        }

        yearMonth = new YearMonth(year, month);
        return true;
    }

    public int CompareTo(YearMonth? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        var yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0)
        {
            return yearComparison;
        }

        return Month.CompareTo(other.Month);
    }
}

public static class DateOnlyExtensions
{
    public static YearMonth ToYearMonth(this DateOnly date) => YearMonth.FromDateOnly(date);
}
