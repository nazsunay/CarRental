using CarRental.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;
using System.Reflection;

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
            using var connection = new SqlConnection(connectionString);
            var cars = connection.Query<Car>("SELECT * FROM Cars").ToList();
    
            return View(cars);
        }
        public IActionResult Details(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var car = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = id });

                if (car == null)
                {
                    return NotFound(); // E�er araba bulunamazsa 404 d�nd�r
                }

                return View(car); // Bulunan arabay� detay g�r�n�m�ne g�nder
            }
        }

        public IActionResult RentNow(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var car = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = id });

                if (car == null)
                {
                    return NotFound(); // E�er araba bulunamazsa 404 d�nd�r
                }

                // Kullan�c� bilgilerini al (�rne�in, oturumdan)
                var customer = GetCurrentCustomer(); // Oturumdaki kullan�c� bilgilerini al

                if (customer == null)
                {
                    return RedirectToAction("Login", "Index"); // Kullan�c� giri�i yap�lmam��sa giri� sayfas�na y�nlendir
                }

                // E-posta g�nderme bilgilerini ayarlay�n
                ViewBag.Subject = "Araba Kiralama Bilgisi";
                ViewBag.Body = $"Merhaba {customer.Name},<br/> {car.Make} {car.Model} kiralama i�leminiz ba�ar�yla ger�ekle�mi�tir.";
                ViewBag.Return = "Index"; // Geri d�n�� sayfas�

                // E-posta g�nder
                SendMail(new Customer { Email = customer.Email, Name = customer.Name });

                return View(car); // Bulunan arabay� detay g�r�n�m�ne g�nder
            }
        }
        public IActionResult SendMail(Customer customer)
        {
            var client = new SmtpClient("smtp.eu.mailgun.org", 587)
            {
                Credentials = new NetworkCredential("postmaster@bildirim.nazlisunay.com.tr", "3b212cffce4a231e162ecd83abce45ea-911539ec-debf1d4c"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("bildirim@rentalCar.com.tr", "rentalCar.com"),
                Subject = ViewBag.Subject,
                Body = ViewBag.Body,
                IsBodyHtml = true,
            };

            mailMessage.ReplyToList.Add(customer.Email);
            mailMessage.To.Add(new MailAddress(customer.Email, customer.Name));

            client.Send(mailMessage);
            return RedirectToAction("Index"); // Geri d�n�� sayfas�
        }
        private Customer GetCurrentCustomer()
        {
            // Session'dan m��teri bilgilerini al
            var customerJson = HttpContext.Session.GetString("CurrentCustomer");

            if (string.IsNullOrEmpty(customerJson))
            {
                return null; // E�er m��teri bilgisi yoksa null d�nd�r
            }

            // JSON'dan Customer nesnesine d�n��t�r
            return JsonConvert.DeserializeObject<Customer>(customerJson);
        }



    }
}
        

