using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Dapper;
using System.Reflection;

namespace CarRental.Controllers
{
    public class RentController : Controller
    {

        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=nazlisun_CarRentalDb; User Id=nazlisun_CarRental;Password=Nazli.55?; TrustServerCertificate=True";

        public IActionResult Index()
        {
            var currentCustomer = GetCurrentCustomer();

            if (currentCustomer == null)
            {
                return RedirectToAction("Login"); // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
            }

            using var connection = new SqlConnection(connectionString);
            var rentals = connection.Query<Rental>("SELECT * FROM Rentals WHERE CustomerId = @CustomerId", new { CustomerId = currentCustomer.Id }).ToList();

            return View(rentals);
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
                    return RedirectToAction("Index", "Login"); // Kullanıcı girişi yapılmamışsa giriş sayfasına yönlendir
                }

                ViewBag.Car = car; // Kiralanacak aracı ViewBag'e ekle

                return View(car); // Kiralama sayfasını göster
            }
        }

        public IActionResult ConfirmRental(int carId, Rental rental, string pickupLocation)
        {
            var customer = GetCurrentCustomer();

            using (var connection = new SqlConnection(connectionString))
            {
                var car = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = carId });
                if (car == null || customer == null)
                {
                    ViewBag.Message = "Gerekli bilgiler eksik.";
                    return View("Error");
                }

                rental.CarId = car.Id;
                rental.CustomerId = customer.Id;
                rental.TotalCost = car.DailyRate;
                rental.PickupLocation = pickupLocation;
                rental.RentalDate = DateTime.Now;
                rental.ReturnDate = rental.ReturnDate;

                var rentalId = connection.ExecuteScalar<int>(
                    "INSERT INTO Rentals (CarId, CustomerId, RentalDate, ReturnDate, TotalCost, PickupLocation) " +
                    "VALUES (@CarId, @CustomerId, @RentalDate, @ReturnDate, @TotalCost, @PickupLocation); SELECT CAST(SCOPE_IDENTITY() as int);",
                    rental
                );

                var subject = "Araba Kiralama Bilgisi";
                var body = $"Merhaba {customer.Name},<br/> {car.Make} {car.Model} kiralama işleminiz başarıyla gerçekleşmiştir.<br/>" +
                           $"Kiralama Tarihi: {rental.RentalDate.ToShortDateString()}<br/>" +
                           $"Teslim Alınacak Konum: {pickupLocation}<br/>" +
                           $"Teslim Tarihi: {rental.ReturnDate.ToShortDateString()}";

                SendMail(new CustomerLoginModel { Email = customer.Email }, subject, body);

                ViewBag.Message = "Mailiniz iletilmiştir.";

                // Index'e yönlendirin ve model olarak Rentals listesini gönderin
                return RedirectToAction("Index");
            }
        }

        //mail düzenlecek !!

        public IActionResult SendMail(CustomerLoginModel customer, string subject, string body)
        {
            try
            {
                var client = new SmtpClient("smtp.eu.mailgun.org", 587)
                {
                    Credentials = new NetworkCredential("postmaster@bildirim.muhammetcoskun.com.tr", "bc4babb0ed21f37fe993af60bd8bbd9f-623e10c8-acec0be3"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("CarRental@bildirim.muhammetcoskun.com.tr", "CarRental.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.ReplyToList.Add(customer.Email);
                mailMessage.To.Add(new MailAddress(customer.Email));

                client.Send(mailMessage);
                return RedirectToAction(ViewBag.Return);
            }
            catch (SmtpException smtpEx)
            {
                // SMTP hatalarını burada işleyebilirsiniz
                Console.WriteLine($"SMTP Hatası: {smtpEx.Message}");
                return View("Error"); // Hata sayfasına yönlendirin
            }
            catch (Exception ex)
            {
                // Diğer hataları burada işleyebilirsiniz
                Console.WriteLine($"Hata: {ex.Message}");
                return View("Error"); // Hata sayfasına yönlendirin
            }
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
