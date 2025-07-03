using System.Collections.Generic;
using Tracker.Domain;

namespace Tracker.Views.Budget;

public record BudgetSummary(IEnumerable<BudgetRowReadModel> Rows, MonthSummary Month);