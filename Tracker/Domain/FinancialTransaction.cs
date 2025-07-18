using System;
using System.Collections.Generic;

namespace Tracker.Domain;

public record ClearedStatus(long Id, string Name)
{
    public ClearedStatus() : this(0L, string.Empty)
    {
    }

    public static readonly ClearedStatus Cleared = new (1, "Cleared");
    public static readonly ClearedStatus Uncleared = new (2, "Uncleared");
}

// account, date, payee, alert, category, memo, outflow, inflow
public record FinancialTransaction(
    long Id,
    long AccountId,
    DateOnly PostedOn,
    string Payee,
    long? CategoryId,
    string Memo,
    decimal Amount,
    Direction Direction,
    ClearedStatus ClearedStatus
)
{
    public FinancialTransaction() :
        this(0L, 0L, default, string.Empty, null, string.Empty, 0m, default, default)
    {
    }

    public static readonly FinancialTransaction Empty = new();

    public static FinancialTransaction CreateNew(
        long accountId,
        DateOnly postedOn,
        string payee,
        long? categoryId,
        string memo,
        decimal amount,
        Direction direction,
        ClearedStatus clearedStatus
    ) => new (0L, accountId, postedOn, payee, categoryId, memo, amount, direction, clearedStatus);
    
    public static FinancialTransaction CreateExisting(
        long id,
        long accountId,
        DateOnly postedOn,
        string payee,
        long? categoryId,
        string memo,
        decimal amount,
        Direction direction,
        ClearedStatus clearedStatus
        )
        => new (id, accountId, postedOn, payee, categoryId, memo, amount, direction, clearedStatus);
}

public record FetchFinancialTransaction(long Id): IQuery<FinancialTransaction?>;
public record FetchFinancialTransactions(long? AccountId = null): IQuery<IEnumerable<FinancialTransaction>>;

public enum Direction { Inflow, Outflow }
public record AddFinancialTransaction(
    long AccountId,
    DateOnly PostedOn,
    string Payee,
    long? CategoryId,
    string Memo,
    decimal Amount,
    Direction Direction,
    ClearedStatus ClearedStatus
);
public record UpdateFinancialTransaction(
    long Id,
    long AccountId,
    DateOnly PostedOn,
    string Payee,
    long? CategoryId,
    string Memo,
    decimal Amount,
    Direction Direction,
    ClearedStatus ClearedStatus
);
public record ImportTransactions(IEnumerable<AddFinancialTransaction> AddFinancialTransactions);
public record RemoveTransaction(long Id);
