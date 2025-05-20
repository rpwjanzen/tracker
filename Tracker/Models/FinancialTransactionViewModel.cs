using Tracker.Domain;

namespace Tracker.Models;

public class FinancialTransactionView
{
    public FinancialTransactionType FinancialTransactionType { get; set; } = FinancialTransaction.Empty;
    public decimal Balance { get; set; }
}