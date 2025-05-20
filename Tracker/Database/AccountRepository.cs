using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class AccountsRepository(DapperContext context) :
    IQueryHandler<FetchAccountsQuery, IEnumerable<AccountType>>,
    ICommandHandler<AddAccount>
{
    public IEnumerable<AccountType> Handle(FetchAccountsQuery query)
    {
        using var connection = context.CreateConnection();
        return connection.Query<(long Id, string Name, string Kind, string BudgetKind, decimal Balance, string Date)>(
            """
            SELECT
                a.id,
                name,
                kind,
                budgetKind,
                coalesce(SUM(CASE WHEN ft.direction = @inflow THEN ft.amount ELSE -ft.amount END), 0.00) + 0.00 as current_balance,
                coalesce(MAX(ft.posted_on), '2025-01-01') as balance_date
            FROM accounts a
            LEFT JOIN financial_transactions ft ON ft.account_id = a.id
            GROUP BY a.id, a.name, a.kind, a.budgetKind
            ORDER BY a.id
            """,
            new
            {
                inflow = Direction.Inflow
            }
        ).Select(x => Account.CreateExisting(
            x.Id,
            x.Name,
            x.Balance,
            DateOnly.Parse(x.Date),
            Enum.Parse<AccountKind>(x.Kind),
            Enum.Parse<BudgetKind>(x.BudgetKind)
            )
        );
    }

    public void Handle(AddAccount command)
    {
        using var connection = context.CreateConnection();
        var accountId = connection.QueryFirst<long>(
            "INSERT INTO accounts (name, kind, budgetKind) VALUES (@name, @kind, @budgetKind) RETURNING id",
            new
            {
                name = command.Name,
                kind = command.Kind,
                budgetKind = command.BudgetKind
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
}