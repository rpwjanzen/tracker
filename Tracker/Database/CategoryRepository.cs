using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class CategoriesRepository :
    IDbQueryHandler<FetchCategoriesQuery, IEnumerable<Category>>,
    IDbQueryHandler<FetchCategoryQuery, Category?>,
    IDbCommandHandler<AddCategory>,
    IDbCommandHandler<RemoveCategory>,
    IDbCommandHandler<RenameCategory>
{
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

    public void Handle(AddCategory command, IDbConnection connection)
    {
        connection.Execute(
            "INSERT INTO categories (name, parent_id) VALUES(@name, @parent_id)",
            new { name = command.Name, parent_id = command.ParentId }
        );
    }

    public void Handle(RemoveCategory command, IDbConnection connection)
    {
        connection.Execute("DELETE FROM categories WHERE id = @id", new { id = command.Id });
    }


    public void Handle(RenameCategory command, IDbConnection connection)
    {
        connection.Execute("UPDATE categories SET name = @name WHERE id = @id", new
        {
            id = command.Id, name = command.NewName
        });
    }
}

