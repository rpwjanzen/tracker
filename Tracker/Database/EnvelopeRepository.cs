using System.Data;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class EnvelopeRepository :
    IDbCommandHandler<UpdateEnvelopeAmount>,
    IDbQueryHandler<FetchEnvelopesQuery, IEnumerable<Envelope>>
{
    public void Handle(UpdateEnvelopeAmount command, IDbConnection connection)
    {
        var rowsAffected = connection.Execute(
            "UPDATE envelopes SET amount = @amount WHERE month = @month AND category_id = @categoryId",
            new
            {
                amount = command.Amount,
                month = command.Month.ToString("yyyy-MM"),
                categoryId = command.CategoryId
            }
        );
        if (rowsAffected == 0)
        {
            connection.Execute(
                "INSERT INTO envelopes (month, amount, category_id)  VALUES (@month, @amount, @categoryId)",
                new
                {
                    month = command.Month.ToString("yyyy-MM"),
                    categoryId = command.CategoryId,
                    amount = command.Amount
                }
            );
        }
    }

    private record EnvelopeRow(long CategoryId, string Month, decimal Amount)
    {
        public EnvelopeRow(long CategoryId, string Month, double Amount)
            : this(CategoryId, Month, (decimal)Amount)
        {
        }

        public EnvelopeRow(long CategoryId, byte[] Month, byte[] Amount) : this(0L, string.Empty, 0m)
        {
            throw new Exception("Cannot convert");
        }
    }
    
    public IEnumerable<Envelope> Handle(FetchEnvelopesQuery query, IDbConnection connection)
    {
        return connection.Query<EnvelopeRow>(
                """
                SELECT
                    c.id as category_id,
                    coalesce(e.month, @month) as month,
                    coalesce(e.amount, 0.00) + 0.00 as amount
                FROM categories c
                    LEFT JOIN envelopes e ON c.id = e.category_id
                WHERE e.month = @month OR e.month IS NULL
                """,
                new
                {
                    month = query.Month.ToString("yyyy-MM")
                }
            )
            .Select(x => new Envelope(DateOnly.ParseExact(x.Month, "yyyy-MM"), x.Amount, x.CategoryId));
    }
}