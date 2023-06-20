using Microsoft.AspNetCore.Mvc;

namespace HouseRent.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }
    }
}
