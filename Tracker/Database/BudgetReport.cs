using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Tracker.Domain;

namespace Tracker.Database;

public class BudgetReport() : IDbQueryHandler<FetchBudgetRowsQuery, IEnumerable<BudgetRowReadModel>>
{
    public IEnumerable<BudgetRowReadModel> Handle(FetchBudgetRowsQuery query, IDbConnection connection)
        => connection.Query<(long, string, string, long, decimal, decimal, decimal)>(
            """
                SELECT
                    c.id as category_id,
                    c.name as category_name,
                    e.month,
                    e.id as envelope_id,
                    e.amount + 0.00 as budgeted,
                    0.00 as overflow,
                    0.00 as balance
                FROM main.categories c
                    JOIN main.categories_envelopes ce ON ce.category_id = c.id
                    JOIN main.envelopes e ON e.id = ce.id
                WHERE e.month = @month
            """,
            new { month = query.Month.ToString() }
        ).Select(t => new BudgetRowReadModel(t.Item1, t.Item2, DateOnly.Parse(t.Item3), t.Item4, t.Item5, t.Item6, t.Item7));
}