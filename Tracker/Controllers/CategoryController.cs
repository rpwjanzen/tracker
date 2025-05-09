using Htmx;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class CategoryController: Controller
{
    private readonly IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> _fetchCategories;
    private readonly IQueryHandler<FetchCategoryQuery, Category?> _fetchCategory;
    private readonly ICommandHandler<AddCategory> _addCategory;
    private readonly ICommandHandler<RemoveCategory> _archiveCategory;
    private readonly ICommandHandler<RenameCategory> _renameCategory;
    
    public CategoryController(
        IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> fetchCategories,
        IQueryHandler<FetchCategoryQuery, Category?> fetchCategory,
        ICommandHandler<AddCategory> addCategory,
        ICommandHandler<RemoveCategory> archiveCategory,
        ICommandHandler<RenameCategory> renameCategory)
    {
        _fetchCategories = fetchCategories;
        _fetchCategory = fetchCategory;
        _addCategory = addCategory;
        _archiveCategory = archiveCategory;
        _renameCategory = renameCategory;
    }
    
    [HttpGet]
    public IActionResult Index() => this.HtmxView("Index", _fetchCategories.Handle(new FetchCategoriesQuery()));

    [HttpGet]
    public IActionResult Add() => this.HtmxView("Add");

    [HttpGet]
    public IActionResult InlineEdit(long id)
    {
        var category = _fetchCategory.Handle(new FetchCategoryQuery(id));
        return PartialView("InlineEdit", category);
    }

    [HttpPatch]
    public IActionResult Index(long id, string name)
    {
        _renameCategory.Handle(new RenameCategory(id, name));
        var category = _fetchCategory.Handle(new FetchCategoryQuery(id));
        if (Request.IsHtmx())
        {
            return PartialView("Inline", category!);
        }

        return NoContent();
    }

    [HttpPost]
    public IActionResult Add(string name, long? parentId)
    {
        _addCategory.Handle(new AddCategory(name, parentId));
        
        if (Request.IsHtmxBoosted())
        {
            return RedirectToAction("Index");
        }

        return Ok();
    }
    
    [HttpDelete]
    public IActionResult Archive(long id)
    {
        _archiveCategory.Handle(new RemoveCategory(id));

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