using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;
using Tracker.Models;

namespace Tracker.Controllers;

/**
 * Other Conventions
 * 
 * HTTP Verb   Path                Controller#Action   Used for
 * GET         /photos             photos#index        display a list of all photos
 * GET         /photos/new         photos#new          return an HTML form for creating a new photo
 * POST        /photos             photos#create       create a new photo
 * GET         /photos/:id         photos#show         display a specific photo
 * GET         /photos/:id/edit    photos#edit         return an HTML form for editing a photo
 * PATCH/PUT   /photos/:id         photos#update       update a specific photo
 * DELETE      /photos/:id         photos#destroy      delete a specific photo
 */

// Conventions:
// GET     Account        gets all accounts
// GET     Account/id     gets the account for the specified id
// POST    Account        creates an account
// PATCH   Account/id     updates an account for the specified id
// DELETE  Account/id     deletes an account for the specified id
// POST    Accounts       creates multiple accounts as per POST body

public class FinancialTransactionController : Controller
{
    private readonly IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransaction>>
        _fetchFinancialTransactionsHandler;
    private readonly IQueryHandler<FetchFinancialTransaction, FinancialTransaction?> _fetchFinancialTransactionHandler;
    private readonly ICommandHandler<AddFinancialTransaction> _addFinancialTransactionService;
    private readonly ICommandHandler<UpdateFinancialTransaction> _updateFinancialTransactionService;
    private readonly ICommandHandler<ImportTransactions> _importTransactions;
    private readonly ICommandHandler<RemoveTransaction> _removeTransaction;
    
    public FinancialTransactionController(
        IQueryHandler<FetchFinancialTransactions, IEnumerable<FinancialTransaction>> fetchFinancialTransactionsHandler,
        IQueryHandler<FetchFinancialTransaction, FinancialTransaction?> fetchFinancialTransactionHandler,
        ICommandHandler<AddFinancialTransaction> addFinancialTransactionService,
        ICommandHandler<UpdateFinancialTransaction> updateFinancialTransactionService,
        ICommandHandler<ImportTransactions> importTransactions,
        ICommandHandler<RemoveTransaction> removeTransaction)
    {
        _fetchFinancialTransactionsHandler = fetchFinancialTransactionsHandler;
        _fetchFinancialTransactionHandler = fetchFinancialTransactionHandler;
        
        _addFinancialTransactionService = addFinancialTransactionService;
        _updateFinancialTransactionService = updateFinancialTransactionService;
        _removeTransaction = removeTransaction;
        
        _importTransactions = importTransactions;
    }

    private IActionResult Index()
    {
        var query = new FetchFinancialTransactions();
        var transactions = _fetchFinancialTransactionsHandler.Handle(query);

        var transactionViews = new List<FinancialTransactionView>();
        var runningTotal = 0m;
        foreach (var transaction in transactions)
        {
            runningTotal += transaction.Amount;
            transactionViews.Add(new FinancialTransactionView
            {
                FinancialTransaction = transaction,
                Balance = runningTotal
            });
        }

        return this.HtmxView("Index", transactionViews);
    }

    [HttpGet]
    public IActionResult Index(long? id)
    {
        if (id is null)
        {
            return Index();
        }
        
        var model = _fetchFinancialTransactionHandler.Handle(new FetchFinancialTransaction(id.Value));
        if (model is null)
        {
            return NotFound();
        }

        return PartialView("Detail", model);
    }
    
    [HttpPost]
    public IActionResult Index([FromBody] AddFinancialTransaction? addFinancialTransaction)
    {
        if (addFinancialTransaction is null)
        {
            return BadRequest();
        }
            
        _addFinancialTransactionService.Handle(addFinancialTransaction);
        return RedirectToAction("Index");
    }
    
    [HttpPut]
    public IActionResult Index(long id, [FromBody] UpdateFinancialTransaction updateFinancialTransaction)
    {
        if (updateFinancialTransaction.Id != id)
        {
            return BadRequest();
        }

        var u = updateFinancialTransaction;
        _updateFinancialTransactionService.Handle(new (u.Id, u.PostedOn, u.Description, u.Amount));
        // TODO: return the row view instead
        return Ok();
    }

    [HttpDelete]
    [ActionName("Index")]
    public IActionResult IndexDelete(long id)
    {
        _removeTransaction.Handle(new RemoveTransaction(id));
        return Ok();
    }

    public IActionResult AddGet() => this.HtmxView("Add");

    [HttpGet]
    public IActionResult Edit(long id)
    {
        var model = _fetchFinancialTransactionHandler.Handle(new(id));
        if (model is null)
        {
            return NotFound();
        }

        return PartialView("Edit", model);
    }
    
    [HttpGet]
    public IActionResult Import() =>this.HtmxView("Import");

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
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}