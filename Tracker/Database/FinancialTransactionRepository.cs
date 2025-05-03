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

    public FinancialTransactionsRepository(DapperContext context) => _context = context;

    class FinancialTransactionRow
    {
        public long transaction_id;
        public string posted_on;
        public string description;
        public decimal amount;
        public long? category_id;
        public string? category_name;
        
        public FinancialTransaction ToFinancialTransaction() =>
            new (transaction_id, ToPostedOn(posted_on), description, amount,
                category_id is not null ? new Category(category_id.Value, category_name!) : null);
    } 
    
    public FinancialTransaction? Handle(FetchFinancialTransaction query)
    {
        using var connection = _context.CreateConnection();
        return connection.QueryFirstOrDefault<FinancialTransactionRow>(
            """
            SELECT
            t.id as transaction_id, posted_on, description, amount + 0.0 as amount, category_id, c.name as category_name
            FROM financial_transactions t
                LEFT JOIN categories c on c.id = t.id
            WHERE t.id = @id
            """,
            new { id = query.Id }
        )?.ToFinancialTransaction();
    }

    public IEnumerable<FinancialTransaction> Handle(FetchFinancialTransactions _)
    {
        using var connection = _context.CreateConnection();
        return connection.Query<FinancialTransactionRow>(
            """
            SELECT t.id as transaction_id, posted_on, description, amount + 0.0 as amount, category_id, c.name as category_name
            FROM financial_transactions t LEFT JOIN categories c on c.id = t.category_id
            ORDER BY posted_on, t.id
            """
        ).Select(t => t.ToFinancialTransaction())
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