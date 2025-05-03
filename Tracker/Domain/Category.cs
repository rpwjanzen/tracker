namespace Tracker.Domain;

public record Category(long Id, string Name)
{
    public static readonly Category Empty = new (0, string.Empty);
}

public record FetchCategoriesQuery : IQuery<IEnumerable<Category>>;
public record AddCategory(string Name);
public record RenameCategory(long Id, string NewName);
public record ArchiveCategory(long Id);
public record ActivateCategory(long Id);