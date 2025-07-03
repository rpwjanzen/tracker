using System.Collections.Generic;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Domain;

namespace Tracker.Controllers;

public sealed class CategoriesController(DapperContext db) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        using var connection = db.CreateConnection();
        var categories = connection.Query<Category>("SELECT id, name FROM categories ORDER BY id");
        return View("Index", new CategoryViewModel(Fragment.List, null, categories));
    }

    [HttpGet("categories/{id:long}")]
    public IActionResult Index(long id)
    {
        var category = FetchCategory(id);
        return PartialView("Index", new CategoryViewModel(Fragment.Details, category, null));
    }

    [HttpGet("categories/add")]
    public IActionResult Add()
        => PartialView("Index", new CategoryViewModel(Fragment.New, Category.Empty, null));

    [HttpPost("categories")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(string name)
    {
        // TODO: if not valid, return Add view with sent contents
        using var connection = db.CreateConnection();
        var id = connection.ExecuteScalar<long>(
            "INSERT INTO categories (name) VALUES (@name) RETURNING id",
            new { name = name }
        );
        
        return PartialView("Index", new CategoryViewModel(Fragment.Details, Category.CreateExisting(id, name), null));
    }

    // not convinced this is the best way to cancel out of add
    [HttpGet("categories/cancel-add")]
    public IActionResult CancelAdd() => Ok();
    
    [HttpGet("categories/{id:long}/edit")]
    public IActionResult EditForm(long id)
    {
        var category = FetchCategory(id);
        return PartialView("Index", new CategoryViewModel(Fragment.Edit, category, null));
    }

    [HttpPatch("categories/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, string name)
    {
        using var connection = db.CreateConnection();
        connection.Execute("UPDATE categories SET name = @name WHERE id = @id", new
        {
            id = id,
            name = name
        });
        
        var category = FetchCategory(id);
        return PartialView("Index", new CategoryViewModel(Fragment.Details, category, null));
    }

    [HttpGet("categories/{id:long}/cancel-edit")]
    public IActionResult CancelEdit(long id)
    {
        var category = FetchCategory(id);
        return PartialView("Index", new CategoryViewModel(Fragment.Details, category, null));
    }

    [HttpDelete("categories/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(long id)
    {
        using var connection = db.CreateConnection();
        connection.Execute("DELETE FROM categories WHERE id = @id", new { id = id });
        return Ok();
    }

    private Category FetchCategory(long id)
    {
        using var connection = db.CreateConnection();
        return connection.QueryFirst<Category>(
            "SELECT id, name FROM categories WHERE id = @id",
            new { id = id }
        );
    }
}

public enum Fragment
{
    New,
    Edit,
    List,
    Details
}
public record CategoryViewModel(Fragment FragmentId, Category? Category, IEnumerable<Category>? Categories);