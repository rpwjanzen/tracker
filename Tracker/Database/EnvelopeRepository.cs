using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class EnvelopeRepository :
    IDbCommandHandler<CreateEnvelope>,
    IDbCommandHandler<CreateEnvelopes>,
    IDbCommandHandler<UpdateEnvelopeAmount>,
    IDbQueryHandler<FetchEnvelopesQuery, IEnumerable<EnvelopeType>>,
    IDbQueryHandler<FetchEnvelopeQuery, OptionType<EnvelopeType>>
{
    public void Handle(CreateEnvelope command, IDbConnection connection)
    {
        var envelopeId = connection.QuerySingle<long>(
            "INSERT INTO envelopes (month, amount)  VALUES (@month, @amount) RETURNING id",
            new
            {
                month = command.Month,
                amount = command.Amount
            }
        );
        
        if (command.CategoryId.HasValue)
        {
            connection.Execute(
                "INSERT INTO categories_envelopes (category_id, envelope_id) VALUES (@categoryId, @envelopeId)",
                new
                {
                    categoryId = command.CategoryId.Value,
                    envelopeId = envelopeId
                }
            );
        }
    }
    
    public void Handle(CreateEnvelopes command, IDbConnection connection)
    {
        var envelopeAndCategoryIds = connection.Query<(long, long)>(
            """
            INSERT INTO envelopes (month, amount)
            SELECT @month, @amount FROM categories c
            WHERE true
            RETURNING id, c.id
            """,
            new
            {
                month = command.Month,
                amount = command.Amount
            }
        );

        foreach (var (envelopeId, categoryId) in envelopeAndCategoryIds)
        {
            connection.Execute(
                """
                INSERT INTO categories_envelopes (category_id, envelope_id)
                VALUES (@categoryId, @envelopeId) 
                """,
                new
                {
                    categoryId = categoryId,
                    envelopeId = envelopeId
                }
            );
        }
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

    public IEnumerable<EnvelopeType> Handle(FetchEnvelopesQuery query, IDbConnection connection)
    {
        return connection.Query<(long Id, string Month, double Amount, long? CategoryId)>(
                """
                SELECT
                    e.id,
                    month,
                    amount + 0.00 as amount,
                    ce.category_id
                FROM envelopes e
                LEFT JOIN categories_envelopes ce ON ce.envelope_id = e.id
                WHERE month = @month
                """,
                new
                {
                    month = query.Month.ToString()
                }
            )
            .Select(x => Envelope.CreateExisting(x.Id, DateOnly.Parse(x.Month), (decimal)x.Amount, x.CategoryId));
    }

    public OptionType<EnvelopeType> Handle(FetchEnvelopeQuery query, IDbConnection connection) =>
        connection.QuerySingleOrDefault<(long Id, string Month, double Amount, long? CategoryId)>(
            """
            SELECT
                    e.id,
                    month,
                    amount + 0.00 as amount,
                    ce.category_id
                FROM envelopes e LEFT JOIN categories_envelopes ce ON ce.envelope_id = e.id
                WHERE e.id = @id
            """,
            new
            {
                id = query.Id,
            }
        ) switch
        {
            (0, _, _, _) => Option.None<EnvelopeType>(),
            var x => Envelope.CreateExisting(x.Id, DateOnly.Parse(x.Month), (decimal)x.Amount, x.CategoryId)
        };
    
    public void Handle(DuplicateBudget command, IDbConnection connection)
    {
        var envelopes = Handle(new FetchEnvelopesQuery(command.SourceMonth), connection);
        foreach (var envelope in envelopes)
        {
            Handle(new CreateEnvelope(command.TargetMonth, envelope.Amount, envelope.CategoryId), connection);
        }
    }
}