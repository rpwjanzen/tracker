using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;
using Tracker.Views.Budget;

namespace Tracker.Controllers;

public class BudgetsController(
    IQueryHandler<FetchBudgetRowsQuery, IEnumerable<BudgetRowReadModel>> fetchBudgetRows
    ) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var month = new DateOnly(2025, 5, 1);
        var budgetRows = fetchBudgetRows.Handle(new FetchBudgetRowsQuery(month));
        var budget = new BudgetSummary(budgetRows, new MonthSummary(month, 0, 0, 0, 0, 0, 0, 0));

        return View("Index", budget);
    }
}