using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Http;
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
                   SUM(ft.amount) AS currentBalance,
                   MAX(ft.posted_on) AS balanceDate,
                   account_type_id as Id,
                   at.name as Name,
                   budget_type_id as Id,
                   bt.name as Name
            FROM accounts a
                JOIN account_types at ON at.id = a.account_type_id
                JOIN budget_types bt ON bt.id = a.budget_type_id
                LEFT JOIN financial_transactions ft ON ft.account_id = a.id
            GROUP BY a.id, a.name, account_type_id, at.name, budget_type_id, bt.name
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
        return View("Index", ForAccount(Fragment.Details, account));
    }

    [HttpGet("accounts/add")]
    public IActionResult Add()
        => View(
            "Index",
            ForAccount(Fragment.New, Account.Empty with { BalanceDate = DateOnly.FromDateTime(DateTime.UtcNow) })
        );

    [HttpPost("accounts/add")]
    [ValidateAntiForgeryToken]
    public IActionResult Add(AddAccountDto dto)
    {
        // TODO: if not valid, return Add view with sent contents
        using var connection = db.CreateConnection();
        connection.ExecuteScalar<long>(
            """
            INSERT INTO accounts (name, account_type_id, budget_type_id)
            VALUES (@name, @accountType, @budgetType)
            RETURNING id
            """,
            new { name = dto.Name, accountType = dto.AccountTypeId, budgetType = dto.BudgetTypeId }
        );
        return Redirect("/accounts");
    }

    public record AddAccountDto(
        string Name,
        long AccountTypeId,
        long BudgetTypeId
    );

    [HttpGet("accounts/{id:long}/edit")]
    public IActionResult EditForm(long id)
    {
        var account = FetchAccount(id);
        return View("Index", ForAccount(Fragment.Edit, account));
    }

    public record EditAccountDto(
        string Name,
        long AccountTypeId,
        long BudgetTypeId
    );
    
    [HttpPost("accounts/{id:long}/edit")]
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
                accountTypeId = dto.AccountTypeId,
                budgetTypeId = dto.BudgetTypeId
            }
        );
        
        return Redirect("/accounts");
    }

    [HttpGet("accounts/{id:long}/delete")]
    public IActionResult DeleteForm(long id)
        => View("Index", ForAccount(Fragment.Delete, FetchAccount(id)));
    
    [HttpPost("accounts/{id:long}/delete")]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(long id)
    {
        using var connection = db.CreateConnection();
        connection.Execute("DELETE FROM financial_transactions WHERE account_id = @id", new { id = id });
        connection.Execute("DELETE FROM accounts WHERE id = @id", new { id = id });
        return Redirect("/accounts");
    }

    private Account FetchAccount(long id)
    {
        using var connection = db.CreateConnection();
        return connection.Query<Account, AccountType, BudgetType, Account>(
            """
            SELECT a.id as Id,
                   a.name,
                   COALESCE(SUM(ft.amount), 0.00) as currentBalance,
                   MAX(ft.posted_on) as balanceDate,
                   account_type_id as Id,
                   at.name,
                   budget_type_id as Id,
                   bt.name
            FROM accounts a
                JOIN account_types at ON at.id = a.account_type_id
                JOIN budget_types bt ON bt.id = a.budget_type_id
                LEFT JOIN financial_transactions ft ON ft.account_id = a.id
            WHERE a.id = @id
            GROUP BY a.id, a.name, account_type_id, at.name, budget_type_id, bt.name
            ORDER BY a.id
            LIMIT 1
            """,
            (account, accountType, budgetType) => account with { Type = accountType, BudgetType = budgetType },
            new { id = id },
            splitOn: "Id"
        ).First();
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

