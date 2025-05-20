using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class AccountController(
    IQueryHandler<FetchAccountsQuery, IEnumerable<AccountType>> fetchAccounts,
    ICommandHandler<AddAccount> addAccount
) : Controller
{
    [HttpGet]
    public IActionResult Index() => PartialView("Index", fetchAccounts.Handle(new FetchAccountsQuery()));
    
    [HttpGet]
    public IActionResult Index(long id)
    {
        return PartialView("Index", fetchAccounts.Handle(new FetchAccountsQuery()));
    }
    
    [HttpGet]
    public IActionResult Add() => PartialView("Add", Account.Empty);

    [HttpPost]
    public IActionResult Add(AddAccount command)
    {
        addAccount.Handle(command);
        return Created();
    }
}