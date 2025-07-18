using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Domain;

namespace Tracker.Controllers;

public record FinancialTransactionView(
    long Id,
    DateOnly PostedOn,
    string Payee,
    decimal Amount,
    Direction Direction,
    string Memo,
    ClearedStatus ClearedStatus,
    AccountView Account,
    Category Category
)
{
    // for Dapper to materialize values that have nulls
    public FinancialTransactionView() :
        this(0L, DateOnly.MinValue, string.Empty, 0M, Direction.Inflow, string.Empty, ClearedStatus.Uncleared,
            AccountView.Empty, Category.Empty)
    {
    }

    public static readonly FinancialTransactionView Empty = new ();
}

public class AccountView
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    
    public static readonly AccountView Empty = new ();
}

public record FinancialTransactionsViewModel(
    Fragment Fragment,
    FinancialTransactionView Transaction,
    IEnumerable<FinancialTransactionView> Transactions,
    IEnumerable<AccountView> Accounts,
    IEnumerable<Category> Categories,
    IEnumerable<ClearedStatus> ClearedStatuses
);

public class FinancialTransactionsController(DapperContext db): Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        using var connection = db.CreateConnection();
        var transactionViews= connection.Query<FinancialTransactionView, AccountView, Category, ClearedStatus, FinancialTransactionView>(
            """
            SELECT ft.id as Id, posted_on, payee, amount, direction, memo,
                   a.id as Id, a.name,
                   c.id as Id, c.name,
                   cs.id as Id, cs.name
            FROM financial_transactions ft
            JOIN accounts a ON a.id = ft.account_id
            JOIN categories c ON ft.category_id = c.id
            JOIN cleared_statuses cs ON cs.id = ft.cleared_status_id
            ORDER BY posted_on, ft.id
            """,
            (transaction, account, category, clearedStatus) => transaction with { Account = account, Category = category, ClearedStatus = clearedStatus }
        );
        
        return View("Index", ForTransactions(Fragment.List, transactionViews, connection));
    }

    // [HttpGet("{id:long}")]
    // public IActionResult Index(long id)
    // {
    //     var model = fetchFinancialTransactionHandler.Handle(new FetchFinancialTransaction(id));
    //     if (model is null)
    //     {
    //         return NotFound();
    //     }
    //
    //     return View("Detail", model);
    // }
    //
    
    [HttpGet]
    public IActionResult Add()
    {
        using var connection = db.CreateConnection();
        return View(
            "Index",
            ForTransaction(
                Fragment.New,
                FinancialTransactionView.Empty with { PostedOn = DateOnly.FromDateTime(DateTime.Today) },
                connection
            )
        );
    }

    public class AddFinancialTransactionDto
    {
        public long AccountId { get; set; }
        public DateOnly PostedOn { get; set; }
        public string? Payee { get; set; }
        public long CategoryId { get; set; }
        public string? Memo { get; set; }
        public decimal Amount { get; set; }
        public Direction Direction { get; set; }
        public long ClearedStatusId { get; set; }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(AddFinancialTransactionDto dto)
    {
        using var connection = db.CreateConnection();
        AddFinancialTransaction(dto, connection);

        return RedirectToAction("Index");
    }
    
    [HttpGet("/financial-transactions/{id:long}/edit")]
    public IActionResult Edit(long id)
    {
        using var connection = db.CreateConnection();
        var transactionView = FetchFinancialTransaction(id, connection);
        if (transactionView == null)
        {
            return NotFound();
        }
        
        return View("Index", ForTransaction(Fragment.Edit, transactionView, connection));
    }
    
    public class UpdateFinancialTransactionDto
    {
        public long AccountId { get; set; }
        public DateOnly PostedOn { get; set; }
        public string Payee { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public string Memo { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public Direction Direction { get; set; }
        public long ClearedStatusId { get; set; }
    }
    
    [HttpPost("/financial-transactions/{id:long}/edit")]
    [ValidateAntiForgeryToken]
    public IActionResult Index(long id, UpdateFinancialTransactionDto dto)
    {
        var connection = db.CreateConnection();
        connection.Execute(
            """
            UPDATE financial_transactions SET 
            posted_on = @postedOn,
            payee = @payee,
            amount = @amount,
            direction = @direction,
            memo = @memo,
            account_id = @accountId,
            cleared_status_id = @clearedStatusId,
            category_id = @categoryId
            WHERE id = @id
            """,
            new
            {
                postedOn = dto.PostedOn,
                payee = dto.Payee ?? string.Empty,
                amount = dto.Amount,
                direction= Direction.Inflow,
                memo = dto.Memo ?? string.Empty,
                accountId = dto.AccountId,
                clearedStatusId = dto.ClearedStatusId,
                categoryId = dto.CategoryId,
                id = id
            }
        );
        
        return Redirect("/financial-transactions");
    }

    [HttpGet("financial-transactions/{id:long}/delete")]
    public IActionResult DeleteForm(long id)
    {
        using var connection = db.CreateConnection();
        var transactionView = FetchFinancialTransaction(id, connection);
        if (transactionView is null)
        {
            return Redirect("/financial-transactions");
        }
        
        return View("Index", ForTransaction(Fragment.Delete, transactionView, connection));
    }

    [HttpPost("financial-transactions/{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(long id)
    {
        using var connection = db.CreateConnection();
        connection.Execute("DELETE FROM financial_transactions WHERE id = @id", new { id = id });
        return Redirect("/financial-transactions");
    }

    private IEnumerable<AccountView> FetchAccountViews(IDbConnection connection)
        => connection.Query<AccountView>("SELECT id, name FROM accounts ORDER BY id");
    
    private FinancialTransactionView? FetchFinancialTransaction(long id, IDbConnection connection)
    {
        var transactionViews = connection.Query<FinancialTransactionView, AccountView, Category, ClearedStatus, FinancialTransactionView>(
            """
            SELECT ft.id as Id, posted_on, payee, amount, direction, memo,
                   a.id as Id, a.name,
                   c.id as Id, c.name,
                   cs.id as Id, cs.name
            FROM financial_transactions ft
            JOIN accounts a ON a.id = ft.account_id
            JOIN categories c ON ft.category_id = c.id
            JOIN cleared_statuses cs ON cs.id = ft.cleared_status_id
            WHERE ft.id = @id
            """,
            (transaction, account, category, clearedStatus) => transaction with { Account = account, Category = category, ClearedStatus = clearedStatus },
            new { id = id }
        );
        return transactionViews.FirstOrDefault();
    }

    [HttpGet("/financial-transactions/import")]
    public IActionResult Import()
    {
        using var connection = db.CreateConnection();
        return View("Import", new ImportTransactionsViewModel { Accounts = FetchAccountViews(connection) });
    }

    [HttpPost("/financial-transactions/import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ImportTransactionsDto dto)
    {
        if (dto.AccountId == 0)
        {
            return BadRequest("Must select account.");
        }
        
        if (dto.File is not { Length: > 0 })
        {
            return BadRequest("File upload failed.");
        }

        var filePath = Path.Combine("wwwroot/uploads", dto.File.FileName);
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        ImportBmoTransactions(filePath, dto.AccountId);

        return Redirect("/financial-transactions");
    }

    private void ImportBmoTransactions(string filePath, long accountId)
    {
        var options = new JsonSerializerOptions
        {
            Converters = { new DecimalConverter() }
        };

        var text = System.IO.File.ReadAllText(filePath);
        var response = JsonSerializer.Deserialize<BmoTransactionsResponse>(text, options);
        Trace.Assert(response is not null);
        var transactions = response.GetBankAccountDetailsRs.BodyResponse.BankAccountTransaction
            .Select(x => new AddFinancialTransactionDto
            {
                AccountId = accountId,
                Amount = Math.Abs(x.Amount),
                CategoryId = x.Amount > 0 ? Category.Income.Id : Category.Uncategorized.Id,
                ClearedStatusId = ClearedStatus.Cleared.Id,
                Direction = x.Amount >= 0 ? Direction.Inflow : Direction.Outflow,
                Memo = string.Empty,
                Payee = FixWhitespace(x.Description),
                PostedOn = x.TransactionDate
            });
        using var connection = db.CreateConnection();
        AddFinancialTransactions(transactions, connection);
    }

    private string FixWhitespace(string text)
    {
        return string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
    
    private FinancialTransactionsViewModel ForTransactions(
        Fragment fragment,
        IEnumerable<FinancialTransactionView> transactions,
        IDbConnection connection
    )
    {
        var accountViews = connection.Query<AccountView>("SELECT id, name FROM accounts ORDER BY id");
        var categoryViews = connection.Query<Category>("SELECT id, name FROM categories ORDER BY id");
        var clearedStatusViews = connection.Query<ClearedStatus>("SELECT id, name FROM cleared_statuses ORDER BY id");

        return new FinancialTransactionsViewModel(
            fragment,
            FinancialTransactionView.Empty,
            transactions,
            accountViews,
            categoryViews,
            clearedStatusViews
        );
    }
    
    private FinancialTransactionsViewModel ForTransaction(
        Fragment fragment,
        FinancialTransactionView transaction,
        IDbConnection connection
    )
    {
        var accountViews = connection.Query<AccountView>("SELECT id, name FROM accounts ORDER BY id");
        var categoryViews = connection.Query<Category>("SELECT id, name FROM categories ORDER BY id");
        var clearedStatusViews = connection.Query<ClearedStatus>("SELECT id, name FROM cleared_statuses ORDER BY id");

        return new FinancialTransactionsViewModel(
            fragment,
            transaction,
            Enumerable.Empty<FinancialTransactionView>(),
            accountViews,
            categoryViews,
            clearedStatusViews
        );
    }

    private void AddFinancialTransaction(AddFinancialTransactionDto dto, IDbConnection connection)
    {
        connection.Execute(
            """
            INSERT INTO financial_transactions (posted_on, payee, amount, direction, memo, account_id, cleared_status_id, category_id)
            VALUES (@postedOn, @payee, @amount, @direction, @memo, @accountId, @clearedStatus, @categoryId)
            """,
            new
            {
                postedOn = dto.PostedOn,
                payee = dto.Payee ?? string.Empty,
                amount = dto.Amount,
                direction = dto.Direction,
                memo = dto.Memo ?? string.Empty,
                accountId = dto.AccountId,
                clearedStatus = dto.ClearedStatusId,
                categoryId = dto.CategoryId
            }
        );
    }
    
    private void AddFinancialTransactions(IEnumerable<AddFinancialTransactionDto> dtos, IDbConnection connection)
    {
        foreach (var dto in dtos)
        {
            AddFinancialTransaction(dto, connection);
        }
    }
}

public class ImportTransactionsDto
{
    public long AccountId { get; set; }
    public IFormFile? File { get; set; }
}

public class ImportTransactionsViewModel
{
    public long AccountId { get; set; }
    public IEnumerable<AccountView> Accounts { get; set; }
}


public class BmoTransactionsResponse
{
    public GetBankAccountDetailsResponse GetBankAccountDetailsRs { get; set; } = new();
}

public class GetBankAccountDetailsResponse
{
    [JsonPropertyName("BodyRs")]
    public BodyResponse BodyResponse { get; set; } = new();
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