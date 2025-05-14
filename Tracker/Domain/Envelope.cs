namespace Tracker.Domain;

public record EnvelopeType(long Id, DateOnly Month, decimal Amount, long CategoryId);

public static class Envelope
{
    public static EnvelopeType CreateNew(DateOnly Month, decimal Amount, long CategoryId)
        => new(0L, Month, Amount, CategoryId);
    public static EnvelopeType CreateExisting(long Id, DateOnly Month, decimal Amount, long CategoryId)
        => new(Id, Month, Amount, CategoryId);
}

public record FetchEnvelopesQuery(DateOnly Month): IQuery<IEnumerable<EnvelopeType>>;
public record FetchEnvelopeQuery(long Id): IQuery<OptionType<EnvelopeType>>;

public record CreateEnvelope(DateOnly Month, decimal Amount, long CategoryId);
public record UpdateEnvelopeAmount(long Id, decimal Amount);
public record RenameEnvelope(long Id, string Name);