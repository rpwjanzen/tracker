using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Models;

namespace Tracker.Controllers;

public class HomeController(DapperContext db) : Controller
{
    public IActionResult Index()
    {
        return View();
        // return RedirectToAction("Index", "Accounts");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult ResetDatabase()
    {
        db.Reset();
        return Redirect("/");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}