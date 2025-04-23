using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class AccountsRepository:
    IQueryHandler<FetchAccountsQuery, IEnumerable<Account>>,
    ICommandHandler<AddAccount>
{
    private readonly DapperContext _context;

    public AccountsRepository(DapperContext context) => _context = context;

    public IEnumerable<Account> Handle(FetchAccountsQuery query)
    {
        using var connection = _context.CreateConnection();
        return connection.Query<Account>("SELECT * FROM accounts ORDER BY id");
    }

    public void Handle(AddAccount command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute(
            "INSERT INTO accounts (name, category) VALUES (@name, @category)",
            new { name = command.Name, category = command.CategoryId }
        );
    }
}