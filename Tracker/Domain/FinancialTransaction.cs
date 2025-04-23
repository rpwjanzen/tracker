namespace Tracker.Domain;

public record FinancialTransaction(long Id, DateOnly PostedOn, string Description, decimal Amount)
{
    public static readonly FinancialTransaction Empty = new(0, DateOnly.MinValue, string.Empty, 0m);
}

public record AddFinancialTransaction(DateOnly PostedOn, string Description, decimal Amount);
public record ImportTransactions(IEnumerable<AddFinancialTransaction> AddFinancialTransactions);
public record RemoveTransaction(long Id);
public record FetchFinancialTransactions: IQuery<IEnumerable<FinancialTransaction>>;