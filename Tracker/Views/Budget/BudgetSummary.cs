using Tracker.Domain;

namespace Tracker.Views.Budget;

public class BudgetSummary
{
    public BudgetSummary(
        IEnumerable<Category> categories,
        IEnumerable<Envelope> envelopes,
        IEnumerable<MonthSummary> months
    )
    {
        var envelopesByCategoryId = envelopes.ToDictionary(x => x.CategoryId);
        
        categories = categories.ToList();
        var categoriesByParentId = categories
            .ToLookup(x => x.ParentId);
        var roots = categories.Where(x => x.ParentId is null);
        Month = months.First();

        var rows = new List<BudgetRow>(categories.Count());
        foreach (var category in roots)
        {
            rows.Add(new BudgetRow(
                category.Name,
                0m.ToString("C"),
                0m.ToString("C"),
                0m.ToString("C"),
                category.ParentId,
                category.Id
            ));

            var subcategories = categoriesByParentId[category.Id];
            foreach (var subcategory in subcategories)
            {
                var envelope = envelopesByCategoryId[subcategory.Id];
                rows.Add(new BudgetRow(
                    "• " + subcategory.Name,
                    envelope.Amount.ToString("C"),
                    "",
                    "",
                    subcategory.ParentId,
                    subcategory.Id
                ));
            }
        }
        
        Rows = rows;
    }

    public record BudgetRow(
        string Name,
        string Budgeted,
        string Outflow,
        string Balance,
        long? ParentId,
        long Id
    );
    
    public IEnumerable<BudgetRow> Rows { get; init; }
    public MonthSummary Month { get; init; }
}
