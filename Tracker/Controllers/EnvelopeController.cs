using Microsoft.AspNetCore.Mvc;
using Tracker.Domain;

namespace Tracker.Controllers;

public class EnvelopeController(
    IQueryHandler<FetchEnvelopeQuery, OptionType<EnvelopeType>> fetchEnvelope,
    ICommandHandler<CreateEnvelope> createEnvelope,
    ICommandHandler<UpdateEnvelopeAmount> updateAmountHandler
) : Controller
{
    [HttpPost]
    public IActionResult Index(string month, decimal amount, long? categoryId)
    {
        createEnvelope.Handle(new CreateEnvelope(DateOnly.Parse(month), amount, categoryId));
        return Created();
    }

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
            .Map<EnvelopeType, IActionResult>(x => PartialView("InlineAmountEditor", (x.Id, x.Amount)))
            .Reduce(NotFound());
    }
}