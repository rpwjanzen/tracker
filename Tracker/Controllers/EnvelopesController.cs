using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Domain;

namespace Tracker.Controllers;

public class EnvelopesController(DapperContext db) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        using var connection = db.CreateConnection();
        var data = new Dictionary<YearMonth, IEnumerable<Envelope>>();
        var year = YearMonth.Now.Year;
        for (var i = 1; i < 13; i++)
        {
            var yearMonth = new YearMonth(year, i);
            var envelopes = FetchEnvelopes(connection, yearMonth);
            data[yearMonth] = envelopes;
        }

        return View("Index", ForEnvelopes(Fragment.List, data[new YearMonth(year, 1)], connection));
    }

    public static IEnumerable<Envelope> FetchEnvelopes(IDbConnection connection)
    {
        var yearMonths =
        return envelopes.AsList();
    }

    public static IEnumerable<Envelope> FetchEnvelopes(IDbConnection connection, YearMonth yearMonth)
    {
        var allEnvelopes = new List<Envelope>();

        var categories = CategoriesController.FetchCategories(connection);
        foreach (var category in categories)
        {
            var envelope = FetchEnvelope(category, yearMonth, connection);
            allEnvelopes.Add(envelope);
        }

        return allEnvelopes;
    }

    private static Envelope FetchEnvelope(Category category, YearMonth yearMonth, IDbConnection connection)
    {
        var sql =
        """
        SELECT e.id as Id,
            @yearMonth as yearMonth,
            e.budgeted,
            COALESCE(SUM(ft.Amount), 0.00) + 0.00 AS allocated,
            c.id as Id,
            c.name as Name
        FROM categories c
            LEFT JOIN financial_transactions ft ON ft.category_id = c.id AND substr(ft.posted_on, 1, 7) = @yearMonth
            LEFT JOIN envelopes e ON e.category_id = c.id
        WHERE e.month = @yearMonth
        GROUP BY coalesce(e.id, 0), @yearMonth, coalesce(e.budgeted, 0.00), c.id, c.name
        """;
        var envelopes = connection.Query<Envelope, Category, Envelope>(
            sql,
            (envelope, category) => envelope with { Category = category },
            new { yearMonth = yearMonth }
        ).AsList();
        if (envelopes.Count == 0)
        {
            return new Envelope(0L, yearMonth, 0M, 0M, category);
        }
        return envelopes.First();
    }

    [HttpGet("envelopes/{id:long}")]
    public IActionResult Index(long id)
    {
        using var connection = db.CreateConnection();
        var envelope = FetchEnvelope(id, connection);
        return View("Index", ForEnvelope(Fragment.Details, envelope, connection));
    }

    [HttpGet("envelopes/add")]
    public IActionResult Add()
    {
        using var connection = db.CreateConnection();
        return View(
            "Index",
            ForEnvelope(Fragment.New, Envelope.Empty with { Month = YearMonth.FromDateTime(DateTime.UtcNow) }, connection)
        );
    }

    [HttpPost("envelopes/add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(AddEnvelopeDto dto)
    {
        // TODO: if not valid, return Add view with sent contents
        using var connection = db.CreateConnection();
        connection.Execute(
            """
            INSERT INTO envelopes (month, budgeted, category_id)
            VALUES (@month, @budgeted, @categoryId)
            """,
            new
            {
                month = dto.Month.ToYearMonth(),
                budgeted = dto.Budgeted,
                categoryId = dto.CategoryId
            }
        );
        return Redirect("/envelopes");
    }

    public record AddEnvelopeDto(
        DateOnly Month,
        decimal Budgeted,
        long CategoryId
    );

    [HttpGet("envelopes/{id:long}/edit")]
    public IActionResult EditForm(long id)
    {
        using var connection = db.CreateConnection();
        var envelope = FetchEnvelope(id, connection);
        return View("Index", ForEnvelope(Fragment.Edit, envelope, connection));
    }

    public record EditEnvelopeDto(
        DateOnly Month,
        decimal Budgeted,
        long CategoryId
    );

    [HttpPost("envelopes/{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, EditEnvelopeDto dto)
    {
        using var connection = db.CreateConnection();
        connection.Execute(
"""
UPDATE envelopes
SET month = @month,
    budgeted = @budgeted,
    category_id = @categoryId
WHERE id = @id 
""",
            new
            {
                id = id,
                month = dto.Month,
                budgeted = dto.Budgeted,
                categoryId = dto.CategoryId
            }
        );

        return Redirect("/envelopes");
    }

    [HttpGet("envelopes/{id:long}/delete")]
    public IActionResult DeleteForm(long id)
    {
        using var connection = db.CreateConnection();
        return View("Index", ForEnvelope(Fragment.Delete, FetchEnvelope(id, connection), connection));
    }

    [HttpPost("envelopes/{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(long id)
    {
        using var connection = db.CreateConnection();
        connection.Execute("DELETE FROM envelopes WHERE id = @id", new { id = id });
        return Redirect("/envelopes");
    }

    public Envelope FetchEnvelope(YearMonth yearMonth, long categoryId, IDbConnection connection)
    {
        return connection.Query<Envelope, Category, Envelope>(
            """
            SELECT coalesce(e.id, 0) as Id,
                   @yearMonth,
                   coalesce(e.budgeted, 0.00) as budgeted,
                   coalesce(SUM(ft.amount), 0.00) AS allocated,
                   @categoryId as Id,
                   c.name as Name
            FROM categories c
                LEFT JOIN envelopes e ON e.category_id = c.id
                LEFT JOIN financial_transactions ft ON ft.category_id = c.id
            WHERE c.id = @categoryId AND (e.month IS NULL OR e.month = @yearMonth)
            GROUP BY coalesce(e.id, 0), @yearMonth, coalesce(e.budgeted, 0.00), @categoryId, c.name
            ORDER BY coalesce(e.id, 0)
            LIMIT 1
            """,
            (envelope, category) => envelope with { Category = category },
            new { yearMonth = yearMonth, categoryId = categoryId }
        ).First();
    }

    public Envelope FetchEnvelope(long id, IDbConnection connection)
    {
        return connection.Query<Envelope, Category, Envelope>(
            """
            SELECT e.id as Id,
                   e.month,
                   e.budgeted,
                   coalesce(SUM(ft.amount), 0.00) AS allocated,
                   e.category_id as Id,
                   c.name as Name
            FROM envelopes e
                JOIN categories c ON c.id = e.category_id
                LEFT JOIN financial_transactions ft ON ft.category_id = c.id
            WHERE e.id = @id
            GROUP BY e.id, e.month, e.budgeted, e.category_id, c.name
            ORDER BY e.id
            LIMIT 1
            """,
            (envelope, category) => envelope with { Category = category },
            new { id = id }
        ).First();
    }

    private EnvelopeViewModel ForEnvelope(
        Fragment fragment,
        Envelope envelope,
        IDbConnection connection
    ) => EnvelopeViewModel.ForEnvelope(fragment, envelope, CategoriesController.FetchCategories(connection));

    private EnvelopeViewModel ForEnvelopes(
        Fragment fragment,
        IEnumerable<Envelope> envelopes,
        IDbConnection connection
    ) => EnvelopeViewModel.ForEnvelopes(fragment, envelopes, CategoriesController.FetchCategories(connection));
}

public record EnvelopeView(Envelope Envelope)
{
    public decimal Balance => Envelope.Budgeted - Envelope.Allocated;
}

public record Envelope(long Id, YearMonth Month, decimal Budgeted, decimal Allocated, Category Category)
{
    public decimal Balance => Budgeted - Allocated;

    public Envelope() : this(0L, YearMonth.MinValue, 0M, 0M, Category.Empty)
    {
    }

    public static readonly Envelope Empty = new();
}

public record EnvelopeViewModel(Fragment FragmentId, EnvelopeView Envelope, IEnumerable<EnvelopeView> Envelopes, IEnumerable<Category> Categories)
{
    public static EnvelopeViewModel ForEnvelope(Fragment fragmentId, Envelope envelope, IEnumerable<Category> categories)
        => new(fragmentId, new EnvelopeView(envelope), Enumerable.Empty<EnvelopeView>(), categories);
    public static EnvelopeViewModel ForEnvelopes(Fragment fragmentId, IEnumerable<Envelope> envelopes, IEnumerable<Category> categories)
        => new(fragmentId, new EnvelopeView(new Envelope()), envelopes.Select(x => new EnvelopeView(x)), categories);
}