using System.Collections.Generic;
using System.Linq;
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
        var view = View("Index", CategoryViewModel.ForCategories(Fragment.List, categories));
        return view;
    }

    [HttpGet("categories/{id:long}")]
    public IActionResult Index(long id)
    {
        var category = FetchCategory(id);
        return View("Index", CategoryViewModel.ForCategory(Fragment.Details, category));
    }

    [HttpGet("categories/add")]
    public IActionResult AddForm()
        => View("Index", CategoryViewModel.ForCategory(Fragment.New, Category.Empty));

    [HttpPost("categories/add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(string name)
    {
        // TODO: if not valid, return Add view with sent contents
        using var connection = db.CreateConnection();
        connection.ExecuteScalar<long>(
            "INSERT INTO categories (name) VALUES (@name)",
            new { name = name }
        );
        
        return Redirect("/categories");
    }

    [HttpGet("categories/{id:long}/edit")]
    public IActionResult EditForm(long id)
    {
        var category = FetchCategory(id);
        return View("Index", CategoryViewModel.ForCategory(Fragment.Edit, category));
    }

    [HttpPost("categories/{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, string name)
    {
        using var connection = db.CreateConnection();
        connection.Execute("UPDATE categories SET name = @name WHERE id = @id", new
        {
            id = id,
            name = name
        });
        return Redirect("/categories");
    }

    [HttpGet("categories/{id:long}/delete")]
    public IActionResult DeleteForm(long id)
        => View("Index", CategoryViewModel.ForCategory(Fragment.Delete, FetchCategory(id)));
    
    [HttpPost("categories/{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(long id)
    {
        using var connection = db.CreateConnection();
        connection.Execute("DELETE FROM financial_transactions WHERE category_id = @id", new { id = id });
        connection.Execute("DELETE FROM categories WHERE id = @id", new { id = id });
        return Redirect("/categories");
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
    Details,
    Delete
}

public record CategoryViewModel(Fragment FragmentId, Category Category, IEnumerable<Category> Categories)
{
    public static CategoryViewModel ForCategory(Fragment fragmentId, Category category)
        => new (fragmentId, category, Enumerable.Empty<Category>());
    public static CategoryViewModel ForCategories(Fragment fragmentId, IEnumerable<Category> categories)
        => new (fragmentId, Category.Empty, categories);
}