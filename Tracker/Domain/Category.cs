namespace Tracker.Domain;

public record CategoryType(long Id, string Name);

public static class Category
{
    public static readonly CategoryType Empty = new(0L, string.Empty);
    
    public static CategoryType CreateNew(string name)
        => new(0L, name);
    public static CategoryType CreateExisting(long id, string name)
        => new(id, name);
}

public record FetchCategoriesQuery : IQuery<IEnumerable<CategoryType>>;
public record FetchCategoryQuery(long Id) : IQuery<OptionType<CategoryType>>;

public record AddCategory(string Name);
public record RenameCategory(long Id, string NewName);
public record RemoveCategory(long Id);