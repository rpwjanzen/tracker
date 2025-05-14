using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class EnvelopeController(
    IQueryHandler<FetchEnvelopeQuery, OptionType<EnvelopeType>> fetchEnvelope,
    ICommandHandler<UpdateEnvelopeAmount> updateAmountHandler
) : Controller
{

    [HttpGet]
    public IActionResult Index(long id)
    {
        return fetchEnvelope.Handle(new FetchEnvelopeQuery(id))
            .Map<EnvelopeType, IActionResult>(x => PartialView("Envelope", x))
            .Reduce(NotFound());
    }

    [HttpPatch]
    public IActionResult Index(long id, decimal amount)
    {
        updateAmountHandler.Handle(new UpdateEnvelopeAmount(id, amount));
        return fetchEnvelope.Handle(new FetchEnvelopeQuery(id))
            .Map<EnvelopeType, IActionResult>(x => PartialView("InlineAmountEditor", x))
            .Reduce(NotFound());
    }
}