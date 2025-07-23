using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Domain;

namespace Tracker.Controllers;

public class ReportsController(DapperContext db) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("ReportsList");
    }
    
    [HttpGet]
    public IActionResult AnnualSpendingByCategory()
    {
        using var connection = db.CreateConnection();
        var rows = connection.Query<MonthlyCategorySummary, Category, MonthlyCategorySummary>(
            // """
            // SELECT e.month,
            //        coalesce(SUM(ft.amount), 0.00) + 0.00 as amount,
            //        ft.category_id as Id,
            //        c.name
            // FROM main.envelopes e
            //     JOIN categories c ON e.category_id = c.id
            //     LEFT JOIN financial_transactions ft ON ft.category_id = c.id
            // WHERE e.month = substr(ft.posted_on, 1, 7)
            // GROUP BY e.month, c.name, ft.category_id
            // ORDER BY e.month, c.name, SUM(ft.amount) desc 
            // """,
            """
            SELECT
                e.month,
                coalesce(SUM(ft.amount), 0.00) + 0.00 as amount,
                c.id,
                c.name
            FROM main.envelopes e
                     JOIN categories c ON e.category_id = c.id
                     LEFT JOIN financial_transactions ft ON ft.category_id = c.id AND substr(ft.posted_on, 1,7) = e.month
            GROUP BY c.name, c.id, e.month
            ORDER BY e.month, c.name
            """,
            (view, category) => view with { Category = category }
        );
        return View("AnnualSpendingByCategoryReport", new AnnualSpendingByCategoryViewModel(rows));
    }

    public record MonthlyCategorySummary(long Id, YearMonth Month, decimal Amount, Category Category)
    {
        public MonthlyCategorySummary() : this(0L, YearMonth.MinValue, 0M, Domain.Category.Empty)
        {
        }
    }
    public record AnnualSpendingByCategoryViewModel(IEnumerable<MonthlyCategorySummary> MonthlySummaries);
}