using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CEMS.Infrastructure;

/// <summary>
/// Session-based access control for Razor Pages. Put it on a PageModel class:
///   [SessionAuthorize]                        -> any logged-in user
///   [SessionAuthorize(Roles = "Staff,Admin")] -> only the listed roles
/// Anonymous users are sent to the login page; users with the wrong role to the Access Denied page.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SessionAuthorizeAttribute : Attribute, IPageFilter
{
    public string? Roles { get; set; }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context) { }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (session.GetInt32("UserId") == null)
        {
            context.Result = new RedirectToPageResult("/Account/Login");
            return;
        }

        if (!string.IsNullOrWhiteSpace(Roles))
        {
            var allowed = Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var role = session.GetString("UserRole") ?? "";
            if (!allowed.Contains(role))
                context.Result = new RedirectToPageResult("/AccessDenied");
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context) { }
}
