using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Tracker.Controllers;

public static class ControllerExtensions
{
    public static IActionResult HtmxView(this Controller controller, [AspMvcView, LocalizationRequired(false)] string viewName)
    {
        return controller.PartialView(viewName);
    }

    public static IActionResult HtmxView(this Controller controller, [AspMvcView, LocalizationRequired(false)] string viewName, [AspMvcModelType] object? model)
    {
        return controller.PartialView(viewName, model);
    }
}