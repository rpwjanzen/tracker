namespace Tracker.Domain;

public record Category(long Id, string Name)
{
    public static readonly Category Empty = new(0L, string.Empty);
    
    public static Category CreateNew(string name) => new(0L, name);
    public static Category CreateExisting(long id, string name) => new(id, name);
}