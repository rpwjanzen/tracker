namespace Tracker.Domain;

public class Category
{
    public long Id { get; init; } 
    public string Name { get; init; } = string.Empty;
    
    public static readonly Category Empty = new();
    public static readonly Category Uncategorized = new () { Id = 0L, Name = "Uncategorized" }; 
    public static readonly Category Income = new () { Id = 1L, Name = "Income" }; 
    
    public static Category CreateNew(string name) => new() { Name = name };
    public static Category CreateExisting(long id, string name) => new () { Id = id, Name = name };
}