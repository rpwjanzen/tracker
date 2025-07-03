using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class AccountsRepository(DapperContext context) :
    IQueryHandler<FetchAccountsQuery, IEnumerable<Account>>,
    IDbQueryHandler<FetchAccountQuery, Account?>,
    ICommandHandler<AddAccount>,
    IDbCommandHandler<UpdateAccount>
{
    public IEnumerable<Account> Handle(FetchAccountsQuery query)
    {
        using var connection = context.CreateConnection();
        var rows = connection.Query(
            """
            SELECT
                a.id,
                a.name,
                account_types.id as account_type_id,
                account_types.name as account_type_name,
                budget_types.id as budget_type_id,
                budget_types.name as budget_type_name,
                coalesce(SUM(CASE WHEN ft.direction = @inflow THEN ft.amount ELSE -ft.amount END), 0.00) + 0.00 as current_balance,
                coalesce(MAX(ft.posted_on), '2025-01-01') as balance_date
            FROM accounts a
                JOIN account_types ON account_types.id = a.account_type_id
                JOIN budget_types ON budget_types.id = a.budget_type_id
                LEFT JOIN financial_transactions ft ON ft.account_id = a.id
            GROUP BY a.id, a.name, account_type_id, account_type_name, budget_type_id, budget_type_name
            ORDER BY a.id
            """,
            new
            {
                inflow = Direction.Inflow
            }
        );
        
        var accounts = rows.Select<dynamic, Account>(x => Account.CreateExisting(
            x.id,
            x.name,
            (decimal)x.current_balance,
            DateOnly.Parse(x.balance_date),
            new AccountKind(x.account_type_id, x.account_type_name),
            new BudgetKind(x.budget_type_id, x.budget_type_name)
        ));
        return accounts;
    }

    public Account? Handle(FetchAccountQuery query, System.Data.IDbConnection connection)
    {
        var row = connection.QueryFirstOrDefault(
            """
            SELECT 
                a.id,
                a.name,
                account_types.id as account_type_id,
                account_types.name as account_type_name,
                budget_types.id as budget_type_id,
                budget_types.name as budget_type_name,
                coalesce(SUM(CASE WHEN ft.direction = @inflow THEN ft.amount ELSE -ft.amount END), 0.00) + 0.00 as balance,
                coalesce(MAX(ft.posted_on), '2025-01-01') as date
            FROM accounts a
            LEFT JOIN financial_transactions ft ON ft.account_id = a.id
            JOIN account_types ON account_types.id = a.account_type_id
            JOIN budget_types ON budget_types.id = a.budget_type_id
            WHERE a.id = @id
            """,
            new
            {
                inflow = Direction.Inflow,
                id = query.Id
            }
        );

        if (row == null)
        {
            return null;
        }

        return Account.CreateExisting(
            row.id,
            row.name,
            (decimal)row.balance,
            DateOnly.Parse(row.date),
            new AccountKind(row.account_type_id, row.account_type_name),
            new BudgetKind(row.budget_type_id, row.budget_type_name)
        );
    }


    public void Handle(AddAccount command)
    {
        using var connection = context.CreateConnection();
        var accountId = connection.QueryFirst<long>(
            "INSERT INTO accounts (name, account_type_id, budget_type_id) VALUES (@name, @type, @budgetKind) RETURNING id",
            new
            {
                name = command.Name.ToString(),
                type = command.KindId,
                budgetKind = command.BudgetKindId
            }
        );
        var transactionId = connection.QueryFirst<long>(
            """
            INSERT INTO financial_transactions (posted_on, payee, amount, direction, memo, account_id, cleared_status)
            VALUES (@postedOn, @payee, @amount, @direction, @memo, @accountId, @clearedStatus)
            RETURNING id
            """,
            new
            {
                postedOn = command.BalanceDate.ToString(),
                payee = "Initial Balance",
                amount = command.CurrentBalance,
                direction = command.CurrentBalance < 0 ? Direction.Outflow : Direction.Inflow,
                memo = string.Empty,
                accountId = accountId,
                clearedStatus = ClearedStatus.Uncleared
            }
        );
    }

    public void Handle(UpdateAccount command, IDbConnection connection)
    {
        connection.Execute(
            "UPDATE accounts SET name = @name, account_type_id = @accountTypeId, budget_type_id = @budgetTypeId WHERE id = @id",
            new
            {
                name = command.Name,
                accountTypeId = command.KindId,
                budgetTypeId = command.BudgetKindId,
                id = command.Id
            }
        );
    }
}

public class FetchBudgetKindsHandler : IDbQueryHandler<FetchBudgetKindsQuery, IEnumerable<BudgetKind>>
{
    public IEnumerable<BudgetKind> Handle(FetchBudgetKindsQuery query, System.Data.IDbConnection connection)
    {
        return connection.Query<BudgetKind>("SELECT id, name FROM budget_types ORDER BY id");
    }
}

public class FetchAccountKindsHandler : IDbQueryHandler<FetchAccountKindsQuery, IEnumerable<AccountKind>>
{
    public IEnumerable<AccountKind> Handle(FetchAccountKindsQuery query, System.Data.IDbConnection connection)
    {
        return connection.Query<AccountKind>("SELECT id, name FROM account_types ORDER BY id");
    }
}