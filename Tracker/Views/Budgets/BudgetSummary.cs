using System.Collections.Generic;
using Tracker.Controllers;
using Tracker.Domain;

namespace Tracker.Views.Budget;

public record BudgetSummary(IEnumerable<Envelope> Rows, MonthSummary Month);