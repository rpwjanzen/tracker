using Htmx;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class AccountsController: Controller
{
    private readonly IQueryHandler<FetchAccountsQuery, IEnumerable<Account>> _fetchAccounts;
    private readonly ICommandHandler<AddAccount> _addAccount;
    
    public AccountsController(
        IQueryHandler<FetchAccountsQuery, IEnumerable<Account>> fetchAccounts,
        ICommandHandler<AddAccount> addAccount)
    {
        _fetchAccounts = fetchAccounts;
        _addAccount = addAccount;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return View(_fetchAccounts.Handle(new FetchAccountsQuery()));
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View(Account.Empty);
    }

    [HttpPost]
    public IActionResult Add(AddAccount command)
    {
        _addAccount.Handle(command);
        return PartialView("_AddAccount");
        // return RedirectToAction("Index");
    }
}