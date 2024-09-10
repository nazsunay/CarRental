using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Dapper;

namespace CarRental.Controllers
{
    public class RentController : Controller
    {

        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=nazlisun_CarRentalDb; User Id=nazlisun_CarRental;Password=Nazli.55?; TrustServerCertificate=True";
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RentNow(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var car = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = id });

                if (car == null)
                {
                    return NotFound(); // Eğer araba bulunamazsa 404 döndür
                }

                var customer = GetCurrentCustomer(); // Oturumdaki kullanıcı bilgilerini al

                if (customer == null)
                {
                    return RedirectToAction("Login", "Index"); // Kullanıcı girişi yapılmamışsa giriş sayfasına yönlendir
                }

                ViewBag.Car = car; // Kiralanan aracı ViewBag ile gönder
                return View(); // Kiralama sayfasını göster
            }
        }

        [HttpPost]
        public IActionResult ConfirmRental(DateTime rentalDate)
        {
            var customer = GetCurrentCustomer();
            var car = ViewBag.Car as Car; // Kiralanan aracı ViewBag'den al

            if (car == null || customer == null)
            {
                return RedirectToAction("Login", "Index"); // Gerekli bilgiler yoksa giriş sayfasına yönlendir
            }

            // Kiralama işlemini veritabanına ekle
            using (var connection = new SqlConnection(connectionString))
            {
                var rental = new Rental
                {
                    CarId = car.Id,
                    CustomerId = customer.Id,
                    RentalDate = rentalDate,
                    ReturnDate = rentalDate.AddDays(1), // Örnek olarak 1 gün kiralama
                    TotalCost = car.DailyRate // Toplam maliyet
                };

                var rentalId = connection.ExecuteScalar<int>(
                    "INSERT INTO Rentals (CarId, CustomerId, RentalDate, ReturnDate, TotalCost) " +
                    "VALUES (@CarId, @CustomerId, @RentalDate, @ReturnDate, @TotalCost); SELECT CAST(SCOPE_IDENTITY() as int);",
                    rental
                );

                // E-posta gönderme bilgilerini ayarlayın
                ViewBag.Subject = "Araba Kiralama Bilgisi";
                ViewBag.Body = $"Merhaba {customer.Name},<br/> {car.Make} {car.Model} kiralama işleminiz başarıyla gerçekleşmiştir. Kiralama Tarihi: {rental.RentalDate.ToShortDateString()}";

                // E-posta gönder
                SendMail(new Customer { Email = customer.Email, Name = customer.Name });

                ViewBag.Message = "Mailiniz iletilmiştir."; // Kullanıcıya gösterilecek mesaj
                return View("Index"); // Ana sayfaya döndür
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
            return RedirectToAction("Index"); // Geri dönüş sayfası
        }

        private Customer GetCurrentCustomer()
        {
            var customerJson = HttpContext.Session.GetString("CurrentCustomer");

            if (string.IsNullOrEmpty(customerJson))
            {
                return null; // Eğer müşteri bilgisi yoksa null döndür
            }

            return JsonConvert.DeserializeObject<Customer>(customerJson);
        }
    }
}
