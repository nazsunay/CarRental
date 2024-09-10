using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
