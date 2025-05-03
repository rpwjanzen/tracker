namespace Tracker.Domain;

public record FinancialTransaction(long Id, DateOnly PostedOn, string Description, decimal Amount, Category? Category)
{
    public FinancialTransaction(long Id, DateOnly PostedOn, string Description, decimal Amount)
        : this(Id, PostedOn, Description, Amount, null)
    {
    }

    public static readonly FinancialTransaction Empty = new(0, DateOnly.MinValue, string.Empty, 0m, Category.Empty);
}

public record FetchFinancialTransaction(long Id): IQuery<FinancialTransaction?>;
public record FetchFinancialTransactions: IQuery<IEnumerable<FinancialTransaction>>;

public record AddFinancialTransaction(DateOnly PostedOn, string Description, decimal Amount);
public record UpdateFinancialTransaction(long Id, DateOnly PostedOn, string Description, decimal Amount);
public record ImportTransactions(IEnumerable<AddFinancialTransaction> AddFinancialTransactions);
public record RemoveTransaction(long Id);
