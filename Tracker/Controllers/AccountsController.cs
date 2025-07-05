using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Tracker.Database;
using Tracker.Domain;

namespace Tracker.Controllers;

public class AccountsController(DapperContext db) : Controller
{
        [HttpGet]
    public IActionResult Index()
    {
        using var connection = db.CreateConnection();
        var accounts = connection.Query<Account, AccountType, BudgetType, Account>(
            """
            SELECT a.id as Id,
                   a.name,
                   account_type_id as Id,
                   at.name as Name,
                   budget_type_id as Id,
                   bt.name as Name
            FROM accounts a
                JOIN account_types at ON at.id = a.account_type_id
                JOIN budget_types bt ON bt.id = a.budget_type_id
            ORDER BY a.id
            """,
            (account, accountType, budgetType) => account with { Type = accountType, BudgetType = budgetType },
            splitOn: "Id"
        );
            
        return View("Index", ForAccounts(Fragment.List, accounts));
    }

    [HttpGet("accounts/{id:long}")]
    public IActionResult Index(long id)
    {
        var account = FetchAccount(id);
        return PartialView("Index", ForAccount(Fragment.Details, account));
    }

    [HttpGet("accounts/add")]
    public IActionResult Add()
        => PartialView(
            "Index",
            ForAccount(Fragment.New, Account.Empty with { BalanceDate = DateOnly.FromDateTime(DateTime.UtcNow) })
        );

    [HttpPost("accounts")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(AddAccountDto dto)
    {
        // TODO: if not valid, return Add view with sent contents
        using var connection = db.CreateConnection();
        var accountId = connection.ExecuteScalar<long>(
            """
            INSERT INTO accounts (name, account_type_id, budget_type_id)
            VALUES (@name, @accountType, @budgetType)
            RETURNING id
            """,
            new { name = dto.Name, accountType = dto.AccountType, budgetType = dto.BudgetType }
        );
        
        // create the initial financial transaction that provides the starting balance
        connection.Execute(
            """
            INSERT INTO financial_transactions (posted_on, payee, amount, direction, memo, account_id, cleared_status) 
            VALUES (@postedOn, @payee, @amount, @direction, @memo, @accountId, @clearedStatus)
            """,
            new
            {
                postedOn = dto.BalanceDate,
                payee = "Initial Balance",
                amount = dto.CurrentBalance,
                direction = Direction.Inflow,
                memo = string.Empty,
                accountId = accountId,
                clearedStatus = ClearedStatus.Cleared
            }
        );
        
        return PartialView("Index", ForAccount(
            Fragment.Details,
            new Account(accountId, dto.Name, dto.CurrentBalance, dto.BalanceDate, dto.AccountType, dto.BudgetType)
        ));
    }

    public record AddAccountDto(
        string Name,
        decimal CurrentBalance,
        DateOnly BalanceDate,
        AccountType AccountType,
        BudgetType BudgetType
    );

    [HttpGet("accounts/cancel-add")]
    public IActionResult CancelAdd() => Ok();
    
    [HttpGet("accounts/{id:long}/edit")]
    public IActionResult EditForm(long id)
    {
        var account = FetchAccount(id);
        return PartialView("Index", ForAccount(Fragment.Edit, account));
    }

    public record EditAccountDto(
        string Name,
        AccountType AccountType,
        BudgetType BudgetType
    );
    
    [HttpPut("accounts/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, EditAccountDto dto)
    {
        using var connection = db.CreateConnection();
        connection.Execute(
"""
UPDATE accounts
SET name = @name,
    account_type_id = @accountTypeId,
    budget_type_id = @budgetTypeId
WHERE id = @id 
""",
            new
            {
                id = id,
                name = dto.Name,
                accountTypeId = dto.AccountType,
                budgetTypeId = dto.BudgetType
            }
        );
        
        var account = FetchAccount(id);
        return PartialView("Index", ForAccount(Fragment.Details, account));
    }

    [HttpGet("accounts/{id:long}/cancel-edit")]
    public IActionResult CancelEdit(long id)
    {
        var account = FetchAccount(id);
        return PartialView("Index", ForAccount(Fragment.Details, account));
    }

    [HttpDelete("accounts/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(long id)
    {
        using var connection = db.CreateConnection();
        connection.Execute("DELETE FROM accounts WHERE id = @id", new { id = id });
        return Ok();
    }

    private Account FetchAccount(long id)
    {
        using var connection = db.CreateConnection();
        return connection.QueryFirst<Account>(
            "SELECT id, name, account_type_id, budget_type_id FROM accounts WHERE id = @id",
            new { id = id }
        );
    }
    
    private IEnumerable<AccountType> FetchAccountTypes()
    {
        using var connection = db.CreateConnection();
        return connection.Query<AccountType>("SELECT id, name FROM account_types ORDER BY name");
    }
    
    private IEnumerable<BudgetType> FetchBudgetTypes()
    {
        using var connection = db.CreateConnection();
        return connection.Query<BudgetType>("SELECT id, name FROM budget_types ORDER BY name");
    }
    
    private AccountViewModel ForAccount(
        Fragment fragment,
        Account account
    ) => AccountViewModel.ForAccount(fragment, account, FetchAccountTypes(), FetchBudgetTypes());
    
    private AccountViewModel ForAccounts(
        Fragment fragment,
        IEnumerable<Account> accounts
    ) => AccountViewModel.ForAccounts(fragment, accounts, FetchAccountTypes(), FetchBudgetTypes());
}

public record AccountViewModel(
    Fragment FragmentId,
    Account Account,
    IEnumerable<Account> Accounts,
    IEnumerable<AccountType> AccountTypes,
    IEnumerable<BudgetType> BudgetTypes
)
{
    public static AccountViewModel ForAccount(
        Fragment fragment,
        Account account,
        IEnumerable<AccountType> accountTypes,
        IEnumerable<BudgetType> budgetTypes
    ) => new(fragment, account, Enumerable.Empty<Account>(), accountTypes, budgetTypes);
    
    public static AccountViewModel ForAccounts(
        Fragment fragment,
        IEnumerable<Account> accounts,
        IEnumerable<AccountType> accountTypes,
        IEnumerable<BudgetType> budgetTypes
    ) => new(fragment, Account.Empty, accounts, accountTypes, budgetTypes);
}