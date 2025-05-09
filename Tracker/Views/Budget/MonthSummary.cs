namespace Tracker.Views.Budget;

public class MonthSummary(
    DateOnly date,
    decimal notBudgeted,
    decimal overspent,
    decimal income,
    decimal budgeted,
    decimal availableToBudget,
    decimal outflows,
    decimal balance
)
{
    public string Month = date.ToString("MMM");
    public string Year = date.ToString("yyyy");
    public string PreviousMonth = date.AddMonths(-1).ToString("MMM");

    public string NotBudgeted = notBudgeted.ToString("C");
    public string Overspent = overspent.ToString("C");
    public string Income = income.ToString("C");
    public string Budgeted = budgeted.ToString("C");
    public string AvailableToBudget = availableToBudget.ToString("C");

    public string Outflows = outflows.ToString("C");
    public string Balance = balance.ToString("C");
}
