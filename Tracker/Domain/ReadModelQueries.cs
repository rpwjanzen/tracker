using System;
using System.Collections.Generic;
using Tracker.Controllers;

namespace Tracker.Domain;

public record BudgetRowReadModel(
    Envelope Envelope,
    DateOnly Month,
    long EnvelopeId,
    decimal Budgeted,
    decimal Outflow,
    decimal Balance,
    Category Category
);

public record FetchBudgetRowsQuery(DateOnly Month) : IQuery<IEnumerable<BudgetRowReadModel>>;