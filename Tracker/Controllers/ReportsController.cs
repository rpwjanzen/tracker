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
        
        return View("AnnualSpendingByCategoryReport", new AnnualSpendingByCategoryViewModel(rows, null));
    }

    [HttpGet]
    public IActionResult MonthlySpendingByCategory(string? month)
    {
        var sql = """
                  SELECT
                      e.month,
                      coalesce(SUM(ft.amount), 0.00) + 0.00 as amount,
                      c.id,
                      c.name
                  FROM main.envelopes e
                           JOIN categories c ON e.category_id = c.id
                           LEFT JOIN financial_transactions ft ON ft.category_id = c.id AND substr(ft.posted_on, 1,7) = e.month
                  WHERE 1 = 1
                  """;
        YearMonth? yearMonth = null;
        if (!string.IsNullOrEmpty(month))
        {
            yearMonth = YearMonth.Parse(month);
            month = yearMonth.ToString();
            
            sql += " AND e.month = @month";
        }
        
        sql += " GROUP BY c.name, c.id, e.month";
        sql += " ORDER BY e.month, c.name";
        
        using var connection = db.CreateConnection();
        var rows = connection.Query<MonthlyCategorySummary, Category, MonthlyCategorySummary>(
            sql,
            (view, category) => view with { Category = category },
            new { month = month }
        );

        return View("MonthlySpendingByCategoryReport", new AnnualSpendingByCategoryViewModel(rows, yearMonth));
    }
    
    [HttpGet]
    public IActionResult Chart(string date)
    {
        // TODO: use SVG instead cause it's the right way t do it.
        // https://www.smashingmagazine.com/2015/07/designing-simple-pie-charts-with-css/
        var yearMonth = YearMonth.Parse(date);
        
        using var connection = db.CreateConnection();
        var rows = connection.Query<MonthlyCategorySummary, Category, MonthlyCategorySummary>(
            """
            SELECT
                e.month,
                coalesce(SUM(ft.amount), 0.00) + 0.00 as amount,
                c.id,
                c.name
            FROM main.envelopes e
                     JOIN categories c ON e.category_id = c.id
                     LEFT JOIN financial_transactions ft ON ft.category_id = c.id AND substr(ft.posted_on, 1,7) = e.month
            WHERE e.month = @month AND c.id <> 1 AND c.id <> 0
            GROUP BY c.name, c.id, e.month
            ORDER BY e.month, c.name
            """,
            (view, category) => view with { Category = category },
            new { month = yearMonth }
        ).AsList();

        var totalAmount = rows.Sum(x => x.Amount);

        var grayColours = new[]
        {
            "#0a0a0a",
            "#1f1f1f",
            "#333333",
            "#474747",
            "#5c5c5c",
            "#707070",
            "#858585",
            "#999999",
            "#adadad",
            "#c2c2c2",
            "#d6d6d6",
            "#ebebeb",
        };
        // https://coolors.co/palette/001219-005f73-0a9396-94d2bd-e9d8a6-ee9b00-ca6702-bb3e03-ae2012-9b2226
        var colours = new[]
        {
            "#001219",
            "#005F73",
            "#0A9396",
            "#94D2BD",
            "#E9D8A6",
            "#EE9B00",
            "#CA6702",
            "#BB3E03",
            "#AE2012",
            "#9B2226",
        };

        var entries = new List<ChartEntry>();
        var nonZeroRows = rows.Where(x => Math.Truncate((x.Amount / totalAmount) * 100) != 0).ToList();
        var offsetAngleInDegrees = 0M;
        for (var i = 0; i < nonZeroRows.Count; i++)
        {
            var row = nonZeroRows[i];
            
            var fraction = row.Amount / totalAmount;
            // 0-100M
            var percent = fraction * 100M;
            // 0-158
            var sweep = fraction * 158M;
            // 0-360
            var startAngle = offsetAngleInDegrees;
            var colour = colours[i % colours.Length];

            entries.Add(new ChartEntry(
                colour,
                (double)Math.Truncate(percent),
                row.Category.Name,
                Math.Truncate(startAngle).ToString("N0") + "deg",
                sweep.ToString("N0"))
            );
            offsetAngleInDegrees += fraction * 360M;
        }

        if (360M - offsetAngleInDegrees > 1)
        {
            var percent = 1M - offsetAngleInDegrees / 360M;
            var sweep = percent * 158M;
            var colour = colours[colours.Length - 1];
            entries.Add(new ChartEntry(
                colour,
                (double)Math.Truncate(percent),
                "Other",
                Math.Truncate(offsetAngleInDegrees).ToString("N0") + "deg",
                sweep.ToString("N0"))
            );
        }
        
        return View("Chart", new ChartViewModel(entries, string.Empty));
    }

    public record MonthlyCategorySummary(long Id, YearMonth Month, decimal Amount, Category Category)
    {
        public MonthlyCategorySummary() : this(0L, YearMonth.MinValue, 0M, Domain.Category.Empty)
        {
        }
    }
    public record AnnualSpendingByCategoryViewModel(IEnumerable<MonthlyCategorySummary> MonthlySummaries, YearMonth? YearMonth);

    public record ChartEntry(string Colour, double Percent, string Description, string CircleOffset, string CircleValue);
    public record ChartViewModel(IList<ChartEntry> Entries, string ConicGradient);
}