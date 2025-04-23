namespace Tracker.Domain;

public record Account(long Id, string Name, long Category)
{
    public static readonly Account Empty = new Account(0, string.Empty, 0);
}

// values are used in DB
public enum AccountCategory
{
    Debit = 1,
    Credit = 2,
}

public record FetchAccountsQuery : IQuery<IEnumerable<Account>>;

public record AddAccount(string Name, long CategoryId);
