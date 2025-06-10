using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RAerp.Controllers.Admin
{
    public class ErrorController : Controller
    {
        public IActionResult ErrorPage(int statusCode)
        {
            var viewString = "";

            // working with return NotFound()
            if (statusCode == 404)
            {
                viewString = "ErrorNotFound";
            }
            else if (statusCode == 500)
            {
                viewString = "ErrorInternalServer";
            }
            else
            {
                viewString = "ErrorOccured";
            }
            // add more return view for errors

            return View(viewString);
        }
    }
}
