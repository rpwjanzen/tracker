using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Domain;

namespace Tracker.Controllers;

public class AccountsController(
    IQueryHandler<FetchAccountsQuery, IEnumerable<Account>> fetchAccounts,
    IQueryHandler<FetchAccountQuery, Account?> fetchAccount,
    IQueryHandler<FetchAccountKindsQuery, IEnumerable<AccountKind>> fetchAccountKinds,
    IQueryHandler<FetchBudgetKindsQuery, IEnumerable<BudgetKind>> fetchBudgetKinds,
    ICommandHandler<AddAccount> addAccount,
    ICommandHandler<UpdateAccount> updateAccount
) : Controller
{
    // [HttpGet("/account")]
    public IActionResult Index()
    {
        var accounts = fetchAccounts.Handle(new FetchAccountsQuery());
        return View("Index", accounts);
    }

    [HttpGet("{id:long}")] // manual path to avoid ambiguous handler method
    public IActionResult Index(long id)
    {
        var account = fetchAccount.Handle(new FetchAccountQuery(id));
        if (account == null)
        {
            return NotFound();
        }
        return View("Detail", account);
    }
    
    [HttpGet]
    public IActionResult Add()
    {
        var dto = AddAccountViewModel.Empty;
        PopulateSelects(dto);
        return View("Add", dto);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(AddAccountViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // rerender form, showing errors to user
            PopulateSelects(viewModel);
            return View("Add", viewModel);
        }

        addAccount.Handle(new AddAccount(viewModel.Name, viewModel.CurrentBalance, viewModel.BalanceDate, viewModel.KindId, viewModel.BudgetKindId));
        return RedirectToAction("Index");
    }
    
    [HttpGet]
    public IActionResult Edit(long id)
    {
        var account = fetchAccount.Handle(new FetchAccountQuery(id));
        if (account == null)
        {
            return NotFound();
        }

        var viewModel = new EditAccountViewModel()
        {
            Id = account.Id,
            Name = account.Name,
            BalanceDate = account.BalanceDate,
            CurrentBalance = account.CurrentBalance,
            BudgetKindId = account.BudgetKind.Id,
            KindId = account.Kind.Id,
        };
        PopulateSelects(viewModel);
        return View("Edit", viewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditAccountViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // rerender form, showing errors to user
            PopulateSelects(viewModel);
            return View("Edit", viewModel);
        }

        updateAccount.Handle(new UpdateAccount(viewModel.Id, viewModel.Name, viewModel.KindId, viewModel.BudgetKindId));
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Archive(long id)
    {
        return RedirectToAction("Index");
    }

    private void PopulateSelects(AddAccountViewModel viewModel)
    {
        viewModel.Kinds = fetchAccountKinds.Handle(new FetchAccountKindsQuery())
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == viewModel.KindId));
        viewModel.BudgetKinds = fetchBudgetKinds.Handle(new FetchBudgetKindsQuery())
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == viewModel.BudgetKindId));
    }
    
    private void PopulateSelects(EditAccountViewModel viewModel)
    {
        viewModel.Kinds = fetchAccountKinds.Handle(new FetchAccountKindsQuery())
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == viewModel.KindId));
        viewModel.BudgetKinds = fetchBudgetKinds.Handle(new FetchBudgetKindsQuery())
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), x.Id == viewModel.BudgetKindId));
    }
}

public class AddAccountViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [DisplayName(displayName: "Current Balance")]
    public decimal CurrentBalance { get; set; }

    [DisplayName(displayName: "Balance Date")]
    [DataType(DataType.Date)]
    public DateOnly BalanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    [DisplayName(displayName: "Kind")]
    public long KindId { get; set; }
    
    [DisplayName(displayName: "Budget Kind")]
    public long BudgetKindId { get; set; }

    public static readonly AddAccountViewModel Empty = new();
    public IEnumerable<SelectListItem> Kinds { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> BudgetKinds { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class EditAccountViewModel
{
    [Required]
    public long Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [DisplayName(displayName: "Current Balance")]
    public decimal CurrentBalance { get; set; }

    [DisplayName(displayName: "Balance Date")]
    [DataType(DataType.Date)]
    public DateOnly BalanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    
    [DisplayName(displayName: "Kind")]
    public long KindId { get; set; }
    
    [DisplayName(displayName: "Budget Kind")]
    public long BudgetKindId { get; set; }

    public static readonly EditAccountViewModel Empty = new();
    public IEnumerable<SelectListItem> Kinds { get; set; } = Enumerable.Empty<SelectListItem>();
    public IEnumerable<SelectListItem> BudgetKinds { get; set; } = Enumerable.Empty<SelectListItem>();
}