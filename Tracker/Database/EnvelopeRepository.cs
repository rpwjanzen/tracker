using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class EnvelopeRepository :
    IDbCommandHandler<CreateEnvelope>,
    IDbCommandHandler<UpdateEnvelopeAmount>,
    IDbQueryHandler<FetchEnvelopesQuery, IEnumerable<EnvelopeType>>,
    IDbQueryHandler<FetchEnvelopeQuery, OptionType<EnvelopeType>>
{
    public void Handle(CreateEnvelope command, IDbConnection connection)
    {
        connection.Execute(
                "INSERT INTO envelopes (month, amount, category_id)  VALUES (@month, @amount, @categoryId)",
                new
                {
                    month = command.Month,
                    categoryId = command.CategoryId,
                    amount = command.Amount
                }
            );
    }

    public void Handle(UpdateEnvelopeAmount command, IDbConnection connection)
    {
        connection.Execute(
            "UPDATE envelopes SET amount = @amount WHERE id = @id",
            new
            {
                id = command.Id,
                amount = command.Amount,
            }
        );
    }

    public record EnvelopeRow(long Id, string Month, double Amount, long CategoryId)
    {
        // public EnvelopeRow(long CategoryId, string Month, double Amount)
        //     : this(CategoryId, Month, (decimal)Amount)
        // {
        // }
    }
    
    public IEnumerable<EnvelopeType> Handle(FetchEnvelopesQuery query, IDbConnection connection)
    {
        return connection.Query<EnvelopeRow>(
                """
                SELECT
                    id,
                    month,
                    amount + 0.00 as amount,
                    category_id
                FROM envelopes e
                WHERE e.month = @month
                """,
                new
                {
                    month = query.Month.ToString()
                }
            )
            .Select(x => Envelope.CreateExisting(x.Id, DateOnly.Parse(x.Month), (decimal)x.Amount, x.CategoryId));
    }

    public OptionType<EnvelopeType> Handle(FetchEnvelopeQuery query, IDbConnection connection)
    {
        return connection.QueryFirstOrDefault<EnvelopeRow>(
            """
            SELECT
                    id,
                    month,
                    amount + 0.00 as amount,
                    category_id
                FROM envelopes
                WHERE id = @id
                LIMIT 1
            """,
            new
            {
                id = query.Id,
            }
        ).ToOption()
        .Map(x => Envelope.CreateExisting(x.Id, DateOnly.Parse(x.Month), (decimal)x.Amount, x.CategoryId));
    }
}