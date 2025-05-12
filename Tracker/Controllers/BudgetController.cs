using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;
using Tracker.Views.Budget;

namespace Tracker.Controllers;

public class BudgetController : Controller
{
    private readonly IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> _fetchCategories;
    private readonly IQueryHandler<FetchEnvelopesQuery, IEnumerable<Envelope>> _fetchEnvelopes;
    
    public BudgetController(
        IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> fetchCategories,
        IQueryHandler<FetchEnvelopesQuery, IEnumerable<Envelope>> fetchEnvelopes
    )
    {
        _fetchCategories = fetchCategories;
        _fetchEnvelopes = fetchEnvelopes;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        var month = new DateOnly(2025, 5, 1);
        var categories = _fetchCategories.Handle(new FetchCategoriesQuery());
        var envelopes = _fetchEnvelopes.Handle(new FetchEnvelopesQuery(month));

        var months = new[]
        {
            new MonthSummary(month, 0, 0, 0, 0, 0, 0, 0),
        };
        var budget = new BudgetSummary(categories, envelopes, months);
        return View("Index", budget);
    }
}