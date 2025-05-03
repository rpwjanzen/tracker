using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class CategoriesRepository :
    IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>>,
    ICommandHandler<AddCategory>,
    ICommandHandler<ArchiveCategory>
{
    private readonly DapperContext _context;

    public CategoriesRepository(DapperContext context) => _context = context;

    public IEnumerable<Category> Handle(FetchCategoriesQuery query)
    {
        using var connection = _context.CreateConnection();
        return connection.Query<Category>(@"SELECT * FROM categories");
    }

    public void Handle(AddCategory command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute("INSERT INTO categories (name) VALUES(@name)", new { name = command.Name });
    }

    public void Handle(ArchiveCategory command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute("DELETE FROM categories WHERE id = @id", new { id = command.Id });
    }
}
