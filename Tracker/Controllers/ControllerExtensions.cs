using Htmx;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Tracker.Controllers;

public static class ControllerExtensions
{
    public static IActionResult HtmxView(this Controller controller, [AspMvcView, LocalizationRequired(false)] string viewName)
    {
        if (controller.Request.IsHtmx())
        {
            return controller.PartialView(viewName);
        }
        return controller.View(viewName);
    }
    
    public static IActionResult HtmxView(this Controller controller, [AspMvcView, LocalizationRequired(false)] string viewName, [AspMvcModelType] object? model)
    {
        if (controller.Request.IsHtmx())
        {
            return controller.PartialView(viewName, model);
        }
        return controller.View(viewName, model);
    }
}