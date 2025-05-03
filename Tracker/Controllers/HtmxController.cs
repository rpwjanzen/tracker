using Htmx;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Tracker.Controllers;

public abstract class HtmxController: Controller
{
    public IActionResult HtmxView([AspMvcAction, LocalizationRequired(false)] string viewName)
    {
        if (Request.IsHtmx())
        {
            return PartialView(viewName);
        }
        return View(viewName);
    }
    
    public IActionResult HtmxView([AspMvcAction, LocalizationRequired(false)] string viewName, [AspMvcModelType] object? model)
    {
        if (Request.IsHtmx())
        {
            return PartialView(viewName, model);
        }
        return View(viewName, model);
    }
}