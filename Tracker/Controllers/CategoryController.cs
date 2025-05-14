using Htmx;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class CategoryController(
    IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> fetchCategories,
    IQueryHandler<FetchCategoryQuery, Category?> fetchCategory,
    ICommandHandler<AddCategory> addCategory,
    ICommandHandler<RemoveCategory> archiveCategory,
    ICommandHandler<RenameCategory> renameCategory) : Controller
{
    [HttpGet]
    public IActionResult Index() => this.HtmxView("Index", fetchCategories.Handle(new FetchCategoriesQuery()));

    [HttpGet]
    public IActionResult Add() => this.HtmxView("Add");

    [HttpGet]
    public IActionResult InlineEditName(long id)
    {
        var category = fetchCategory.Handle(new FetchCategoryQuery(id));
        return PartialView("InlineEditName", category);
    }

    [HttpPatch]
    public IActionResult Index(long id, string name)
    {
        renameCategory.Handle(new RenameCategory(id, name));
        var category = fetchCategory.Handle(new FetchCategoryQuery(id));
        if (Request.IsHtmx())
        {
            return PartialView("Inline", category!);
        }

        return NoContent();
    }

    [HttpPost]
    public IActionResult Add(string name, long? parentId)
    {
        addCategory.Handle(new AddCategory(name, parentId));
        
        if (Request.IsHtmxBoosted())
        {
            return RedirectToAction("Index");
        }

        return Ok();
    }
    
    [HttpDelete]
    public IActionResult Archive(long id)
    {
        archiveCategory.Handle(new RemoveCategory(id));

        if (Request.IsHtmxBoosted())
        {
            return Ok();
        }
        else
        {
            return RedirectToAction("Index");
        }
    }
}