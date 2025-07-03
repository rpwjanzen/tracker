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
    long KindId,
    long BudgetKindId
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
    AccountKind Kind,
    BudgetKind BudgetKind
)
{
    public static readonly Account Empty = new(0L, string.Empty, 0m, default, AccountKind.Empty, BudgetKind.Empty);

    public static Account CreateNew(
        string name,
        decimal currentBalance,
        DateOnly balanceDate,
        AccountKind kind,
        BudgetKind budgetKind)
        => new(0L, name, currentBalance, balanceDate, kind, budgetKind);

    public static Account CreateExisting(
        long id,
        string name,
        decimal currentBalance,
        DateOnly balanceDate,
        AccountKind kind,
        BudgetKind budgetKind)
        => new(id, name, currentBalance, balanceDate, kind, budgetKind);
}

public record AccountKind(long Id, string Name)
{
    public static readonly AccountKind Empty = new (0L, string.Empty);
}

public record FetchAccountKindsQuery : IQuery<IEnumerable<AccountKind>>;

public record BudgetKind(long Id, string Name)
{
    public static readonly BudgetKind Empty = new (0L, string.Empty);
}

public record FetchBudgetKindsQuery : IQuery<IEnumerable<BudgetKind>>;