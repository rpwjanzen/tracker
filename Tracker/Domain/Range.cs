namespace Tracker.Domain;

public record YearMonthRange(YearMonth Start, YearMonth End)
{
    public int TotalYears => End.Year - Start.Year;

    public int TotalMonths => (End.Year * 12 + End.Month) - (Start.Year * 12 + Start.Month);
}
