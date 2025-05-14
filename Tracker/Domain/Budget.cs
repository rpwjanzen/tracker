namespace Tracker.Domain;

public class Budget;

public record Envelope(DateOnly Month, decimal Amount, long CategoryId);

public record FetchEnvelopesQuery(DateOnly Month): IQuery<IEnumerable<Envelope>>;
public record FetchEnvelopeQuery(DateOnly Month, long CategoryId): IQuery<OptionType<Envelope>>;

public record UpdateEnvelopeAmount(
    DateOnly Month,
    long CategoryId,
    decimal Amount
);