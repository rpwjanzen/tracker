namespace Tracker.Domain;

public enum AccountKind
{
    Checking,
    Savings,
    CreditCard,
    Cash,
    LineOfCredit,
    Paypal,
    MerchantAccount,
    InvestmentAccount,
    Mortgage,
    OtherAsset, // house, car, etc
    OtherLoanOrLiability
}

public enum BudgetKind
{
    Budget,
    OffBudget
}

public record AccountType(
    long Id,
    string Name,
    decimal CurrentBalance,
    DateOnly BalanceDate,
    AccountKind Kind,
    BudgetKind BudgetKind
);

public record FetchAccountsQuery : IQuery<IEnumerable<AccountType>>;

//  name, current balance, date of current balance, kind, budget/off-budget account
public record AddAccount(
    string Name,
    decimal CurrentBalance,
    DateOnly BalanceDate,
    AccountKind Kind,
    BudgetKind BudgetKind
);

public static class Account
{
    public static readonly AccountType Empty = new (0L, string.Empty, 0m, default, default, default);

    public static AccountType CreateNew(
        string name,
        decimal currentBalance,
        DateOnly balanceDate,
        AccountKind kind,
        BudgetKind budgetKind)
        => new (0L, name, currentBalance, balanceDate, kind, budgetKind);
    
    public static AccountType CreateExisting(
        long id,
        string name,
        decimal currentBalance,
        DateOnly balanceDate,
        AccountKind kind,
        BudgetKind budgetKind)
        => new (id, name, currentBalance, balanceDate, kind, budgetKind);
}
