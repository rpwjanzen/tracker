using System;
using System.Collections.Generic;

namespace Tracker.Domain;

public record BudgetRowReadModel(
    long CategoryId,
    string CategoryName,
    DateOnly Month,
    long EnvelopeId,
    decimal Budgeted,
    decimal Outflow,
    decimal Balance
);

public record FetchBudgetRowsQuery(DateOnly Month) : IQuery<IEnumerable<BudgetRowReadModel>>;