using Tracker.Domain;

namespace Tracker.Models;

public class FinancialTransactionView
{
    public FinancialTransaction FinancialTransaction { get; set; } = FinancialTransaction.Empty;
    public decimal Balance { get; set; }
}