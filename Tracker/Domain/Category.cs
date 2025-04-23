namespace Tracker.Domain;

public record Category(long Id, string Name)
{
    public static readonly Category Empty = new Category(0, string.Empty);
}

public record FetchCategoriesQuery : IQuery<IEnumerable<Category>>;