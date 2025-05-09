using System.Globalization;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class FinancialTransactionsRepository :
    IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransaction>>,
    IQueryHandler<FetchFinancialTransaction, FinancialTransaction?>,
    ICommandHandler<AddFinancialTransaction>,
    ICommandHandler<UpdateFinancialTransaction>,
    ICommandHandler<ImportTransactions>,
    ICommandHandler<RemoveTransaction>
{
    private readonly DapperContext _context;

    public FinancialTransactionsRepository(DapperContext context)
    {
        _context = context;
    }

    public FinancialTransaction? Handle(FetchFinancialTransaction query)
    {
        using var connection = _context.CreateConnection();
        return connection.QueryFirstOrDefault<FinancialTransaction>(
            """
            SELECT
            t.id,
            posted_on,
            description,
            amount + 0.0 as amount
            FROM financial_transactions t
                LEFT JOIN categories c on c.id = t.id
            WHERE t.id = @id
            """,
            new { id = query.Id }
        );
    }

    private record FinancialTransactionRow(long Id, string PostedOn, string Description, decimal Amount)
    {
        public FinancialTransactionRow(long Id, string PostedOn, string Description, byte[] Amount) : this(Id, PostedOn,
            Description, 0m)
        {
            throw new Exception("" + Amount.Length);
        }
    }
    
    public IEnumerable<FinancialTransaction> Handle(FetchFinancialTransactions _)
    {
        using var connection = _context.CreateConnection();
        return connection.Query<FinancialTransactionRow>(
            """
            SELECT id,
                   posted_on,
                   description,
                   amount + 0.0 as amount
            FROM financial_transactions
            ORDER BY posted_on, id
            """
        ).Select(x => new FinancialTransaction(x.Id, ToPostedOn(x.PostedOn), x.Description, x.Amount))
        .AsList();
    }

    public void Handle(AddFinancialTransaction command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute(
            """
            INSERT INTO financial_transactions (posted_on, description, amount)
            VALUES (@postedOn, @description, @amount)
            """,
            new
            {
                postedOn = command.PostedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                description = command.Description,
                amount = command.Amount.ToString(CultureInfo.InvariantCulture),
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
        using var connection = _context.CreateConnection();
        connection.Execute("DELETE FROM financial_transactions WHERE id = @id", new { id = command.Id });
    }

    public void Handle(UpdateFinancialTransaction command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute(
            """
            UPDATE financial_transactions
            SET posted_on = @postedOn,
                description = @description,
                amount = @amount
            WHERE id = @id
            """,
            new
            {
                id = command.Id, postedOn = command.PostedOn, description = command.Description, amount = command.Amount
            }
        );
    }

    private static DateOnly ToPostedOn(string value) =>
        DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}