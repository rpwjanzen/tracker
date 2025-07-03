using System;
using System.Collections.Generic;

namespace Tracker.Domain;

// account, date, payee, alert, category, memo, outflow, inflow
public record FinancialTransactionType(
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

public static class FinancialTransaction
{
    public static readonly FinancialTransactionType Empty = new(0L, 0L, default, string.Empty, null, string.Empty, 0m, default, default);

    public static FinancialTransactionType CreateNew(
        long accountId,
        DateOnly postedOn,
        string payee,
        long? categoryId,
        string memo,
        decimal amount,
        Direction direction,
        ClearedStatus clearedStatus
    ) => new (0L, accountId, postedOn, payee, categoryId, memo, amount, direction, clearedStatus);
    
    public static FinancialTransactionType CreateExisting(
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

public record FetchFinancialTransaction(long Id): IQuery<FinancialTransactionType?>;
public record FetchFinancialTransactions(long? AccountId = null): IQuery<IEnumerable<FinancialTransactionType>>;

public enum Direction { Inflow, Outflow }
public enum ClearedStatus
{
    Uncleared,
    Cleared
}
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
