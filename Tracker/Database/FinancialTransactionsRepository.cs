using System.Globalization;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class FinancialTransactionsRepository :
    IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransaction>>,
    ICommandHandler<AddFinancialTransaction>,
    ICommandHandler<ImportTransactions>,
    ICommandHandler<RemoveTransaction>
{
    private readonly DapperContext _context;

    public FinancialTransactionsRepository(DapperContext context) => _context = context;

    public IEnumerable<FinancialTransaction> Handle(FetchFinancialTransactions _)
    {
        using var connection = _context.CreateConnection();
        var rows = connection.Query( "SELECT * FROM financial_transactions ORDER BY posted_on, id");
        return rows.Select(row => new FinancialTransaction(
            row.id,
            DateOnly.ParseExact(row.posted_on, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            row.description,
            (decimal)row.amount
        ));
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
        connection.Execute("DELETE FROM financial_transactions WHERE id = @id", command.Id);
    }
}