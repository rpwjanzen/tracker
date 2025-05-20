namespace Tracker.Domain;

public record EnvelopeType(long Id, DateOnly Month, decimal Amount, long? CategoryId);

public static class Envelope
{
    public static readonly EnvelopeType Empty = CreateNew(DateOnly.MinValue, 0m);
    
    public static EnvelopeType CreateNew(DateOnly month, decimal amount)
        => new(0L, month, amount, null);
    public static EnvelopeType CreateExisting(long id, DateOnly month, decimal amount, long? categoryId)
        => new(id, month, amount, categoryId);
}

public record FetchEnvelopesQuery(DateOnly Month): IQuery<IEnumerable<EnvelopeType>>;
public record FetchEnvelopeQuery(long Id): IQuery<OptionType<EnvelopeType>>;

public record CreateEnvelope(DateOnly Month, decimal Amount, long? CategoryId);
public record CreateEnvelopes(DateOnly Month, decimal Amount);
public record UpdateEnvelopeAmount(long Id, decimal Amount);
public record RenameEnvelope(long Id, string Name);