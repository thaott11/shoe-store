using Microsoft.AspNetCore.Mvc;

namespace Shoe_Store.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/Forbidden")]
        public IActionResult Forbidden()
        {
            return View();
        }

        [Route("Error/NotFound")]
        public IActionResult NotFound()
        {
            return View();
        }


        [Route("Error/ServerError")]
        public IActionResult ServerError()
        {
            return View();
        }
        
    }
}
