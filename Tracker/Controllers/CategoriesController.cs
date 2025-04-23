using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class CategoriesController: Controller
{
    private readonly IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> _fetchCategories;

    public CategoriesController(
        IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> fetchCategories
    )
    {
        _fetchCategories = fetchCategories;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View(_fetchCategories.Handle(new FetchCategoriesQuery()));
    }
}