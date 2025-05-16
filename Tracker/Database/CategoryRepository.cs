using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class CategoriesRepository :
    IDbQueryHandler<FetchCategoriesQuery, IEnumerable<CategoryType>>,
    IDbQueryHandler<FetchCategoryQuery, OptionType<CategoryType>>,
    IDbCommandHandler<AddCategory>,
    IDbCommandHandler<RemoveCategory>,
    IDbCommandHandler<RenameCategory>
{
    public OptionType<CategoryType> Handle(FetchCategoryQuery query, IDbConnection connection)
    {
        var (id, name, parentId) = connection.QuerySingleOrDefault<(long id, string name, long? parentId)>(
            "SELECT id, name, parent_id FROM categories WHERE id = @id",
            new { id = query.Id }
        );
        if (id == 0m)
        {
            return Option.None<CategoryType>();
        }
        return Category.CreateExisting(id, name, parentId.ToOption());
    }

    public IEnumerable<CategoryType> Handle(FetchCategoriesQuery query, IDbConnection connection)
    {
        return connection.Query<(long id, string name, long? parent_id)>(
            """
            SELECT c.id, c.name, c.parent_id
            FROM categories c
            ORDER BY c.parent_id, c.id
            """
        ).Select(x => Category.CreateExisting(x.id, x.name, x.parent_id.ToOption()));
    }

    public void Handle(AddCategory command, IDbConnection connection)
    {
        connection.Execute(
            "INSERT INTO categories (name, parent_id) VALUES(@name, @parent_id)",
            new { name = command.Name, parent_id = command.ParentId.ToNullable() }
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
            id = command.Id,
            name = command.NewName
        });
    }
}