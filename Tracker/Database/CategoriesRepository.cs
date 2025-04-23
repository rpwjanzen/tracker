using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class CategoriesRepository: IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>>
{
    private readonly DapperContext _context;

    public CategoriesRepository(DapperContext context) => _context = context;

    public IEnumerable<Category> Handle(FetchCategoriesQuery query)
    {
        using var connection = _context.CreateConnection();
        return connection.Query<Category>(@"SELECT * FROM categories");
    }
}
