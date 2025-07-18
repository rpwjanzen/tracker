using System;
using System.Collections.Generic;

namespace Tracker.Domain;

public record FetchAccountsQuery : IQuery<IEnumerable<Account>>;
public record FetchAccountQuery(long Id) : IQuery<Account?>;


//  name, current balance, date of current balance, kind, budget/off-budget account
public record AddAccount(
    string Name,
    decimal CurrentBalance,
    DateOnly BalanceDate,
    long TypeId,
    long BudgetTypeId
);

public record UpdateAccount(
    long Id,
    string Name,
    long KindId,
    long BudgetKindId
);

public sealed record Account(
    long Id,
    string Name,
    decimal CurrentBalance,
    DateOnly BalanceDate,
    AccountType Type,
    BudgetType BudgetType
)
{
    // for Dapper
    public Account() :
        this(0L, string.Empty, 0M, DateOnly.FromDateTime(DateTime.Today), AccountType.Empty, BudgetType.Empty)
    {
    }
    
    public static readonly Account Empty = new(0L, string.Empty, 0m, default, AccountType.Empty, BudgetType.Empty);
    public static readonly Account Unspecified = Empty with { Name = "Unspecified" };

    public static Account CreateNew(
        string name,
        decimal currentBalance,
        DateOnly balanceDate,
        AccountType type,
        BudgetType budgetType)
        => new(0L, name, currentBalance, balanceDate, type, budgetType);

    public static Account CreateExisting(
        long id,
        string name,
        decimal currentBalance,
        DateOnly balanceDate,
        AccountType type,
        BudgetType budgetType)
        => new(id, name, currentBalance, balanceDate, type, budgetType);
}

public record AccountType(long Id, string Name)
{
    // for Dapper
    public AccountType(): this(0L, string.Empty) {}
    
    public static readonly AccountType Empty = new (0L, string.Empty);
}

public record FetchAccountKindsQuery : IQuery<IEnumerable<AccountType>>;

public record BudgetType(long Id, string Name)
{
    // for Dapper
    public BudgetType() : this(0L, string.Empty)
    {
    }

    public static readonly BudgetType Empty = new (0L, string.Empty);
}

public record FetchBudgetKindsQuery : IQuery<IEnumerable<BudgetType>>;