using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Diagnostics;

namespace CarRental.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=nazlisun_CarRentalDb; User Id=nazlisun_CarRental;Password=Nazli.55?; TrustServerCertificate=True";

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public IActionResult Contact()
        {
            // Boþ bir ContactViewModel nesnesi oluþturuluyor ve View'e geçiliyor
            return View(new ContactViewModel());
        }


        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Verileri iþleme (örneðin, e-posta gönderme)

                ViewBag.Message = "Mesajýnýz baþarýyla gönderildi.";
                return View(model);
            }

            // Model geçerli deðilse, View'i modelle birlikte döndür
            return View(model);
        }


    }
}
