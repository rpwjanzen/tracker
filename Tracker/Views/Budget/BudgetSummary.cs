using Tracker.Domain;

namespace Tracker.Views.Budget;

public class BudgetSummary
{
    public BudgetSummary(
        IEnumerable<CategoryType> categories,
        IEnumerable<EnvelopeType> envelopes,
        IEnumerable<MonthSummary> months
    )
    {
        var envelopesByCategoryId = envelopes.ToDictionary(x => x.CategoryId);

        categories = [.. categories];
        var categoriesByParentId = categories.ToLookup(x => x.ParentId);
        var rootCategories = categories.Where(x => x.ParentId.HasValue);
        Month = months.First();

        var rows = new List<BudgetRow>(categories.Count());
        foreach (var rootCategory in rootCategories)
        {
            rows.Add(new BudgetRow(
                rootCategory,
                Envelope.CreateNew(DateOnly.MinValue, 0m, 0),
                0m.ToString("F"),
                0m.ToString("F")
            ));

            var subcategories = categoriesByParentId[rootCategory.Id];
            foreach (var subcategory in subcategories)
            {
                if (envelopesByCategoryId.TryGetValue(subcategory.Id, out var envelope))
                {
                    rows.Add(new BudgetRow(
                        subcategory,
                        envelope,
                        "",
                        ""
                    ));
                }
            }
        }

        Rows = rows;
    }

    public record BudgetRow(
        CategoryType Category,
        EnvelopeType Envelope,
        string Outflow,
        string Balance
    );

    public IEnumerable<BudgetRow> Rows { get; init; }
    public MonthSummary Month { get; init; }
}
