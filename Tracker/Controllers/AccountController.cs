using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class AccountController(
    IQueryHandler<FetchAccountsQuery, IEnumerable<Account>> fetchAccounts,
    ICommandHandler<AddAccount> addAccount) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return this.HtmxView("Index", fetchAccounts.Handle(new FetchAccountsQuery()));
    }

    [HttpGet]
    public IActionResult Add() => this.HtmxView("Add", Account.Empty);

    [HttpPost]
    public IActionResult Add(AddAccount command)
    {
        addAccount.Handle(command);
        return RedirectToAction("Index");
    }
}