using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;
using Tracker.Models;

namespace Tracker.Controllers;

public class FinancialTransactionsController: Controller
{
    private readonly IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransaction>>
        _fetchFinancialTransactionsHandler;
    private readonly ICommandHandler<AddFinancialTransaction> _addFinancialTransactionService;
    private readonly ICommandHandler<ImportTransactions> _importTransactions;
    private readonly ICommandHandler<RemoveTransaction> _removeTransaction;
    
    public FinancialTransactionsController(
        IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransaction>> fetchFinancialTransactionsHandler,
        ICommandHandler<AddFinancialTransaction> addFinancialTransactionService,
        ICommandHandler<ImportTransactions> importTransactions,
        ICommandHandler<RemoveTransaction> removeTransaction)
    {
        _fetchFinancialTransactionsHandler = fetchFinancialTransactionsHandler;
        _addFinancialTransactionService = addFinancialTransactionService;
        _importTransactions = importTransactions;
        _removeTransaction = removeTransaction;
    }
    
    public IActionResult Index()
    {
        var query = new FetchFinancialTransactions();
        var transactions = _fetchFinancialTransactionsHandler.Handle(query);

        // TODO: use LINQ
        var transactionViews = new List<FinancialTransactionView>();
        var balance = 0m;
        foreach (var transaction in transactions)
        {
            balance += transaction.Amount;
            transactionViews.Add(new FinancialTransactionView()
            {
                FinancialTransaction = transaction,
                Balance = balance
            });
        }
        return View(transactionViews);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Add([FromForm] AddFinancialTransaction addFinancialTransaction)
    {
        _addFinancialTransactionService.Handle(addFinancialTransaction);

        return RedirectToAction("Index");
    }

    public IActionResult Import()
    {
        return View();
    }

    [HttpDelete]
    public IActionResult Remove(long id)
    {
        _removeTransaction.Handle(new RemoveTransaction(id));

        return RedirectToAction("Index");
    }
    
    [HttpPost]
    public async Task<IActionResult> Import(FileUploadModel model)
    {
        if (model.File is not { Length: > 0 })
        {
            return BadRequest("File upload failed.");
        }

        var filePath = Path.Combine("wwwroot/uploads", model.File.FileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        ImportBmoTransactions(filePath);
        
        return RedirectToAction("Index");
    }

    private void ImportBmoTransactions(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new DecimalConverter() }
        };

        var text = System.IO.File.ReadAllText(filePath);
        var response = JsonSerializer.Deserialize<BmoTransactionsResponse>(text, options);
        Trace.Assert(response is not null);
        var transactions = response.GetBankAccountDetailsRs.BodyResponse.BankAccountTransaction
            .Select(t => new AddFinancialTransaction(t.TransactionDate, t.Description, t.Amount));
        _importTransactions.Handle(new ImportTransactions(transactions));
    }
}

public class FileUploadModel
{
    public IFormFile? File { get; set; }
}

public class BmoTransactionsResponse
{
    public GetBankAccountDetailsResponse GetBankAccountDetailsRs { get; set; }= new();
}

public class GetBankAccountDetailsResponse
{
    [JsonPropertyName("BodyRs")]
    public BodyResponse BodyResponse { get; set; } = new ();
}

public class BodyResponse
{
    [JsonPropertyName("bankAccountTransactions")]
    public IEnumerable<BankAccountTransaction> BankAccountTransaction { get; set; } =
        Enumerable.Empty<BankAccountTransaction>();
}

public class BankAccountTransaction
{
    // yyyy-MM-dd
    [JsonPropertyName("txnDate")]
    public DateOnly TransactionDate { get; set; } = DateOnly.MinValue;
    
    // IB, ...
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("descr")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("txnAmount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("balance")]
    public decimal Balance { get; set; }

    // Unused
    // public string cimgReg { get; set; }
}

public class DecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && decimal.TryParse(reader.GetString(), out var value))
        {
            return value;
        }
        
        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}