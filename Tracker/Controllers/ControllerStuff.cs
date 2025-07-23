using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Tracker.Controllers;

public class RequireHeaderAttribute(string headerName) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var headers = context.HttpContext.Request.Headers;
        if (!headers.ContainsKey(headerName))
        {
            context.Result = new ContentResult
            {
                StatusCode = 400,
                Content = $"Missing required header: {headerName}"
            };
        }
    }
}

public class HtmxOnlyAttribute() : RequireHeaderAttribute("HX-Request")
{
}