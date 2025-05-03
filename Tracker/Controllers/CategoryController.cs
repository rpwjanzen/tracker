using Htmx;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class CategoryController: Controller
{
    private readonly IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> _fetchCategories;
    private readonly ICommandHandler<AddCategory> _addCategory;
    private readonly ICommandHandler<ArchiveCategory> _archiveCategory;
    
    public CategoryController(
        IQueryHandler<FetchCategoriesQuery, IEnumerable<Category>> fetchCategories,
        ICommandHandler<AddCategory> addCategory,
        ICommandHandler<ArchiveCategory> archiveCategory)
    {
        _fetchCategories = fetchCategories;
        _addCategory = addCategory;
        _archiveCategory = archiveCategory;
    }
    
    [HttpGet]
    public IActionResult Index() => this.HtmxView("Index", _fetchCategories.Handle(new FetchCategoriesQuery()));

    [HttpGet]
    public IActionResult Add() => this.HtmxView("Add");

    [HttpPost]
    public IActionResult Add(string name)
    {
        _addCategory.Handle(new AddCategory(name));
        
        if (Request.IsHtmxBoosted())
        {
            return RedirectToAction("Index");
        }

        return Ok();
    }
    
    [HttpDelete]
    public IActionResult Archive(long id)
    {
        _archiveCategory.Handle(new ArchiveCategory(id));

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