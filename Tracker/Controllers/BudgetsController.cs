using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Domain;

namespace Tracker.Controllers;

public class BudgetsController(DapperContext db) : Controller
{
    [HttpGet]
    public IActionResult Index(string yearMonth)
    {
        using var connection = db.CreateConnection();
        var envelopes = EnvelopesController.FetchEnvelopes(connection, new YearMonth(2024, 1))
            .GroupBy(x => x.Month);
        var budget = envelopes.ToDictionary(x => x.Key, x => x.AsEnumerable());
            
        return View("Index", budget);
    }
}