using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class EnvelopeController(
    IQueryHandler<FetchEnvelopeQuery, OptionType<Envelope>> fetchEnvelope,
    ICommandHandler<UpdateEnvelopeAmount> updateAmountHandler
) : Controller
{

    [HttpGet]
    public IActionResult Index(DateOnly month, long categoryId)
    {
        return fetchEnvelope.Handle(new FetchEnvelopeQuery(month, categoryId))
            .Map<Envelope, IActionResult>(x => PartialView("Envelope", x))
            .Reduce(NotFound());
    }

    [HttpPatch]
    public IActionResult Index(DateOnly month, long categoryId, decimal amount)
    {
        updateAmountHandler.Handle(new UpdateEnvelopeAmount(month, categoryId, amount));
        return fetchEnvelope.Handle(new FetchEnvelopeQuery(month, categoryId))
            .Map<Envelope, IActionResult>(x => PartialView("InlineAmountEditor", x))
            .Reduce(NotFound());
    }
}