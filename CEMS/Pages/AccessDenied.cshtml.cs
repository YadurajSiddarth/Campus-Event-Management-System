using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CEMS.Pages;

public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
    }
}
