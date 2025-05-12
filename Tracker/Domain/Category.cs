namespace Tracker.Domain;

public record Category(long Id, string Name, long? ParentId)
{
    public static readonly Category Empty = new (0, string.Empty, null);
}

public record FetchCategoriesQuery : IQuery<IEnumerable<Category>>;
public record FetchCategoryQuery(long Id) : IQuery<Category?>;

public record AddCategory(string Name, long? ParentId);
public record RenameCategory(long Id, string NewName);
public record RemoveCategory(long Id);
