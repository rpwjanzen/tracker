using Microsoft.AspNetCore.Mvc;
using Tracker.Views.Budget;

namespace Tracker.Controllers;

public class BudgetController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var months = new[]
        {
            new MonthSummary(new DateOnly(2025, 5, 1), 0, 0, 0, 0, 0),
            new MonthSummary(new DateOnly(2025, 6, 1), 1, 0, 0, 0, 0),
            new MonthSummary(new DateOnly(2025, 7, 1), 2, 0, 0, 0, 0),
            new MonthSummary(new DateOnly(2025, 8, 1), 3, 0, 0, 0, 0),
        };
        var budget = new BudgetSummary(months);
        return View("Index", budget);
    }
}