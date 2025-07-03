using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class FinancialTransactionsRepository(DapperContext context) :
    IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransactionType>>,
    IQueryHandler<FetchFinancialTransaction, FinancialTransactionType?>,
    ICommandHandler<AddFinancialTransaction>,
    ICommandHandler<UpdateFinancialTransaction>,
    ICommandHandler<ImportTransactions>,
    ICommandHandler<RemoveTransaction>
{
    public FinancialTransactionType? Handle(FetchFinancialTransaction query)
    {
        using var connection = context.CreateConnection();
        return connection
            .Query<(long Id, string PostedOn, string Payee, decimal Amount, string Direction, string Memo,
                long AccountId, string ClearedStatus, long? CategoryId)>(
                """
                SELECT ft.id, posted_on, payee, amount + 0.00 AS amount, direction, memo, account_id, cleared_status, ce.category_id
                FROM financial_transactions ft
                    LEFT JOIN financial_transactions_envelopes fte ON ft.id = fte.financial_transaction_id
                    LEFT JOIN categories_envelopes ce ON ce.envelope_id = fte.envelope_id
                WHERE ft.id = @id
                """,
                new { id = query.Id }
            ).Select(x => FinancialTransaction.CreateExisting(
                x.Id,
                x.AccountId,
                DateOnly.Parse(x.PostedOn),
                x.Payee,
                x.CategoryId,
                x.Memo,
                x.Amount,
                Enum.Parse<Direction>(x.Direction),
                Enum.Parse<ClearedStatus>(x.ClearedStatus))
            ).FirstOrDefault();
    }

    public IEnumerable<FinancialTransactionType> Handle(FetchFinancialTransactions query)
    {
        if (!query.AccountId.HasValue)
        {
            using var connection = context.CreateConnection();
            return connection.Query<(long Id, string PostedOn, string Payee, decimal Amount, string Direction, string Memo, long AccountId, string ClearedStatus, long? CategoryId)>(
                """
            SELECT ft.id, posted_on, payee, amount + 0.00 AS amount, direction, memo, account_id, cleared_status, ce.category_id
            FROM financial_transactions ft
                LEFT JOIN financial_transactions_envelopes fte ON ft.id = fte.financial_transaction_id
                LEFT JOIN categories_envelopes ce ON ce.envelope_id = fte.envelope_id
            ORDER BY posted_on, ft.id
            """
            ).Select(x => FinancialTransaction.CreateExisting(
                x.Id,
                x.AccountId,
                DateOnly.Parse(x.PostedOn),
                x.Payee,
                x.CategoryId,
                x.Memo,
                x.Amount,
                Enum.Parse<Direction>(x.Direction),
                Enum.Parse<ClearedStatus>(x.ClearedStatus))
            );
        }
        else
        {
            using var connection = context.CreateConnection();
            return connection.Query<(long Id, string PostedOn, string Payee, decimal Amount, string Direction, string Memo, long AccountId, string ClearedStatus, long? CategoryId)>(
                """
                SELECT ft.id, posted_on, payee, amount + 0.00 AS amount, direction, memo, account_id, cleared_status, ce.category_id
                FROM financial_transactions ft
                    LEFT JOIN financial_transactions_envelopes fte ON ft.id = fte.financial_transaction_id
                    LEFT JOIN categories_envelopes ce ON ce.envelope_id = fte.envelope_id
                WHERE ft.account_id = @accountId
                ORDER BY posted_on, ft.id
                """,
                new { accountId = query.AccountId.Value }
            ).Select(x => FinancialTransaction.CreateExisting(
                x.Id,
                x.AccountId,
                DateOnly.Parse(x.PostedOn),
                x.Payee,
                x.CategoryId,
                x.Memo,
                x.Amount,
                Enum.Parse<Direction>(x.Direction),
                Enum.Parse<ClearedStatus>(x.ClearedStatus))
            );
        }
    }

    public void Handle(AddFinancialTransaction command)
    {
        using var connection = context.CreateConnection();
        connection.Execute(
            """
            INSERT INTO financial_transactions (posted_on, payee, amount, direction, memo, account_id, cleared_status)
            VALUES (@postedOn, @payee, @outflow, @inflow, @memo, @accountId, @clearedStatus)
            """,
            new
            {
                postedOn = command.PostedOn,
                payee = command.Payee,
                amount = command.Amount,
                direction = command.Direction,
                memo = command.Memo,
                accountId = command.AccountId,
                clearedStatus = command.ClearedStatus
            }
        );
    }

    public void Handle(ImportTransactions command)
    {
        foreach (var cmd in command.AddFinancialTransactions)
        {
            Handle(cmd);
        }
    }

    public void Handle(RemoveTransaction command)
    {
        using var connection = context.CreateConnection();
        connection.Execute("DELETE FROM financial_transactions WHERE id = @id", new { id = command.Id });
    }

    public void Handle(UpdateFinancialTransaction command)
    {
        using var connection = context.CreateConnection();
        connection.Execute(
            """
            UPDATE financial_transactions
            SET
                posted_on = @postedOn,
                payee = @payee,
                amount = @amount,
                direction = @direction,
                memo = @memo,
                cleared_status = @clearedStatus,
                account_id = @accountId
            WHERE id = @id
            """,
            new
            {
                id = command.Id,
                postedOn = command.PostedOn,
                payee = command.Payee,
                amount = command.Amount,
                direction = command.Direction,
                memo = command.Memo,
                clearedStatus = command.ClearedStatus,
                accountId = command.AccountId
            }
        );
    }
}