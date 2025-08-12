using System.Collections.Generic;
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
        using var connetion = db.CreateConnection();
        var envelopes = EnvelopesController.FetchEnvelopes(connetion).GroupBy(x => x.Month);
        var budget = envelopes.ToDictionary(x => x.Key, x => x.AsEnumerable());
            
        // var budget = new Dictionary<YearMonth, IEnumerable<Envelope>>();

        return View("Index", budget);
    }
}