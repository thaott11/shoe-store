using Microsoft.AspNetCore.Mvc;

namespace Shoe_Store.Controllers
{
    public class UserController : Controller
    {
        public IActionResult ProductDetail()
        {
            return View();
        }

        public IActionResult ProductList()
        {
            return View();
        }
    }
}
