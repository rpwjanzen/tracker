using Htmx;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class AccountController: Controller
{
    private readonly IQueryHandler<FetchAccountsQuery, IEnumerable<Account>> _fetchAccounts;
    private readonly ICommandHandler<AddAccount> _addAccount;
    
    public AccountController(
        IQueryHandler<FetchAccountsQuery, IEnumerable<Account>> fetchAccounts,
        ICommandHandler<AddAccount> addAccount)
    {
        _fetchAccounts = fetchAccounts;
        _addAccount = addAccount;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        return this.HtmxView("Index", _fetchAccounts.Handle(new FetchAccountsQuery()));
    }

    [HttpGet]
    public IActionResult Add()
    {
        return this.HtmxView("Add", Account.Empty);
    }

    [HttpPost]
    public IActionResult Add(AddAccount command)
    {
        _addAccount.Handle(command);
        return RedirectToAction("Index");
    }
}