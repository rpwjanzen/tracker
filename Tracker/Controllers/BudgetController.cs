using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;
using Tracker.Views.Budget;

namespace Tracker.Controllers;

public class BudgetController(
    IQueryHandler<FetchCategoriesQuery, IEnumerable<CategoryType>> fetchCategories,
    IQueryHandler<FetchEnvelopesQuery, IEnumerable<EnvelopeType>> fetchEnvelopes
    ) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var month = new DateOnly(2025, 5, 1);
        var categories = fetchCategories.Handle(new FetchCategoriesQuery());
        var envelopes = fetchEnvelopes.Handle(new FetchEnvelopesQuery(month));

        var months = new[]
        {
            new MonthSummary(month, 0, 0, 0, 0, 0, 0, 0),
        };
        var budget = new BudgetSummary(categories, envelopes, months);
        return View("Index", budget);
    }
}