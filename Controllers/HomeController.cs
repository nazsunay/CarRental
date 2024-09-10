using CarRental.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Data.SqlClient;
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

        [HttpGet]
        public IActionResult Index(int? id)
        {
            using var connection = new SqlConnection(connectionString);

            if (id != null)
            {
                var car = connection.Query<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = id.Value }).FirstOrDefault();

                if (car == null)
                {
                    return NotFound(new { msg = "Araç bulunamadý." });
                }

                return Json(car);
            }

            var cars = connection.Query<Car>("SELECT * FROM Cars").ToList();
            return View(cars); // Tüm araçlar View'da döndürülür
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
