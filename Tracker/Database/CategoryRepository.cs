using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class CategoriesRepository :
    IDbQueryHandler<FetchCategoriesQuery, IEnumerable<Category>>,
    IDbQueryHandler<FetchCategoryQuery, Category?>,
    ICommandHandler<AddCategory>,
    ICommandHandler<RemoveCategory>,
    ICommandHandler<RenameCategory>
{
    private readonly DapperContext _context;

    public CategoriesRepository(DapperContext context) => _context = context;
    
    public Category? Handle(FetchCategoryQuery query, IDbConnection connection)
    {
        var category = connection.QuerySingleOrDefault<Category>(
            "SELECT id, name, parent_id FROM categories WHERE id = @id",
            new { id = query.Id }
        );
        return category;
    }

    public IEnumerable<Category> Handle(FetchCategoriesQuery query, IDbConnection connection)
    {
        return connection.Query<Category>(
            """
            SELECT c.id, c.name, c.parent_id
            FROM categories c
            ORDER BY c.parent_id, c.id
            """
        );
    }

    public void Handle(AddCategory command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute(
            "INSERT INTO categories (name, parent_id) VALUES(@name, @parent_id)",
            new { name = command.Name, parent_id = command.ParentId }
        );
    }

    public void Handle(RemoveCategory command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute("DELETE FROM categories WHERE id = @id", new { id = command.Id });
    }


    public void Handle(RenameCategory command)
    {
        using var connection = _context.CreateConnection();
        connection.Execute("UPDATE categories SET name = @name WHERE id = @id", new
        {
            id = command.Id, name = command.NewName
        });
    }
}

// ReSharper disable TypeParameterCanBeVariant
public interface IDbQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    TResult Handle(TQuery query, IDbConnection connection);
}
// ReSharper restore TypeParameterCanBeVariant

public class DbQueryHandlerAdapter<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    private readonly DapperContext _dapperContext;
    private readonly IDbQueryHandler<TQuery, TResult> _queryHandler;
    
    public DbQueryHandlerAdapter(IDbQueryHandler<TQuery, TResult> queryHandler, DapperContext dapperContext)
    {
        _queryHandler = queryHandler;
        _dapperContext = dapperContext;
    }
    
    public TResult Handle(TQuery query)
    {
        using var connection = _dapperContext.CreateConnection();
        return _queryHandler.Handle(query, connection);
    }
}
