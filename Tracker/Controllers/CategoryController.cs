using Htmx;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class CategoryController(
    IQueryHandler<FetchCategoriesQuery, IEnumerable<CategoryType>> fetchCategories,
    IQueryHandler<FetchCategoryQuery, OptionType<CategoryType>> fetchCategory,
    ICommandHandler<AddCategory> addCategory,
    ICommandHandler<RemoveCategory> archiveCategory,
    ICommandHandler<RenameCategory> renameCategory) : Controller
{
    [HttpGet]
    public IActionResult Index() => this.HtmxView("Index", fetchCategories.Handle(new FetchCategoriesQuery()));

    [HttpGet]
    public IActionResult Add() => this.HtmxView("Add");

    [HttpGet]
    public IActionResult InlineEditName(long id) =>
        fetchCategory.Handle(new FetchCategoryQuery(id))
            .Match<CategoryType, IActionResult>(x => PartialView("InlineEditName", x), NotFound);

    [HttpPatch]
    public IActionResult Index(long id, string name)
    {
        renameCategory.Handle(new RenameCategory(id, name));
        return fetchCategory.Handle(new FetchCategoryQuery(id))
            .Match<CategoryType, IActionResult>(x => Request.IsHtmx() ? PartialView("Inline", x) : NoContent(), NotFound);
    }

    [HttpPost]
    public IActionResult Add(string name)
    {
        addCategory.Handle(new AddCategory(name));
        return Request.IsHtmx() ? RedirectToAction("Index") : Ok();
    }

    [HttpDelete]
    public IActionResult Archive(long id)
    {
        archiveCategory.Handle(new RemoveCategory(id));
        return Request.IsHtmx() ? Ok() : RedirectToAction("Index");
    }
}