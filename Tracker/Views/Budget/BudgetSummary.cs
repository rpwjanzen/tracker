namespace Tracker.Views.Budget;

public class BudgetSummary
{
    public BudgetSummary(IEnumerable<MonthSummary> months)
    {
        Months = months;
    }
    
    public IEnumerable<MonthSummary> Months { get; init; }
}
