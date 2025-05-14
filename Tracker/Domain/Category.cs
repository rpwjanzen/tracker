namespace Tracker.Domain;

public record CategoryType(long Id, string Name, long? ParentId);

public static class Category
{
    public static readonly CategoryType Empty = new (0, string.Empty, null);
    public static CategoryType CreateNew(string name, long? parentId)
        => new (0L, name, parentId);
    public static CategoryType CreateExisting(long id, string name, long? parentId)
        => new (id, name, parentId);
}

public record FetchCategoriesQuery : IQuery<IEnumerable<CategoryType>>;
public record FetchCategoryQuery(long Id) : IQuery<OptionType<CategoryType>>;

public record AddCategory(string Name, long? ParentId);
public record RenameCategory(long Id, string NewName);
public record RemoveCategory(long Id);
