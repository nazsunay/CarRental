using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using Dapper;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace CarRental.Controllers
{
    public class RentController : Controller
    {

        

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

                var customer = GetCurrentCustomer();

                if (customer == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                ViewBag.Car = car;

                return View(car);
            }
        }

        public IActionResult ConfirmRental(int carId, Rental rental, string pickupLocation)
        {
            var customer = GetCurrentCustomer();

            using (var connection = new SqlConnection(connectionString))
            {
                var car = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = carId });
                if (car == null || customer == null || !car.IsAvailable)
                {
                    ViewBag.Message = " Araç Kiralanmıştır Lütfen başka bir araç seçiniz .";
                    return View("Message");
                }

                // Kullanıcının zaten bir kiralaması var mı kontrol et
                var existingRentals = connection.Query<Rental>(
                    "SELECT * FROM Rentals WHERE CustomerId = @CustomerId AND " +
                    "((RentalDate <= @ReturnDate AND ReturnDate >= @RentalDate) OR " +
                    "(RentalDate >= @RentalDate AND RentalDate <= @ReturnDate))",
                    new { CustomerId = customer.Id, RentalDate = rental.RentalDate, ReturnDate = rental.ReturnDate }
                ).ToList();

                if (existingRentals.Any())
                {
                    ViewBag.Message = "Bu tarihler arasında araç kiralanmış. Lütfen başka bir araç seçin.";
                    return View("Message");
                }

                // Yeni kiralama işlemi
                rental.CarId = car.Id;
                rental.CustomerId = customer.Id;
                rental.TotalCost = car.DailyRate;
                rental.PickupLocation = pickupLocation;
                rental.RentalDate = DateTime.Now;
                rental.ReturnDate = rental.ReturnDate ?? DateTime.Now.AddDays(1); // Örnek: 1 gün kiralama

                // Araç kiralandıktan sonra IsAvailable'ı false yap
                car.IsAvailable = false;
                connection.Execute("UPDATE Cars SET IsAvailable = @IsAvailable WHERE Id = @Id", new { IsAvailable = car.IsAvailable, Id = car.Id });

                var rentalId = connection.ExecuteScalar<int>(
                    "INSERT INTO Rentals (CarId, CustomerId, RentalDate, ReturnDate, TotalCost, PickupLocation) " +
                    "VALUES (@CarId, @CustomerId, @RentalDate, @ReturnDate, @TotalCost, @PickupLocation); SELECT CAST(SCOPE_IDENTITY() as int);",
                    rental
                );

                var emailContent = CreateEmailContent(rental, car, customer.Email);

                try
                {
                    SendMail(new Customer { Email = customer.Email }, emailContent);
                    ViewBag.Message = "Mailiniz iletilmiştir.";
                }
                catch (Exception)
                {
                    ViewBag.Message = "E-posta gönderiminde bir sorun oluştu.";
                }

                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public IActionResult CancelRental(int rentalId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    // İptal edilecek kiralama bilgisini al
                    var rental = connection.QuerySingleOrDefault<Rental>("SELECT * FROM Rentals WHERE Id = @Id", new { Id = rentalId });
                    if (rental == null)
                    {
                        return NotFound(); // Kiralama bulunamadı
                    }

                    // Aracın durumunu güncelle
                    var carId = rental.CarId;
                    connection.Execute("UPDATE Cars SET IsAvailable = 1 WHERE Id = @Id", new { Id = carId });

                    // Kiralama kaydını sil
                    connection.Execute("DELETE FROM Rentals WHERE Id = @Id", new { Id = rentalId });

                    ViewBag.Message = $"Kiralama ID {rentalId} başarıyla iptal edildi.";
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "İptal işlemi sırasında bir hata oluştu: " + ex.Message;
                }

                return View("Message");
            }
        }

        private string CreateEmailContent(Rental rental, Car car, string customerEmail)
        {
            return $"<h1>Kiralanan aracınız : {car.Make} </h1>" +
                   $"<p>Aracınızın modeli: {car.Model},</p>" +
                   $"<p>Aracı kiralama tarihiniz: {rental.RentalDate.ToShortDateString()} </p>" +
                   $"<h2>Aracı Teslim Tarihiniz: {rental.ReturnDate?.ToShortDateString()} </h2>" +
                   $"<p>Aracı almak istediğiniz yer: {rental.PickupLocation}</p>" +
                   "<h2>En yakın zamanda görüşmek dileğiyle</h2>" +
                   "<p>Herhangi bir sorunuz veya geri bildiriminiz olursa bize bildirin.</p>";
        }


        public IActionResult SendMail(Customer customer, string emailContent)
        {
            try
            {
                var client = new SmtpClient("smtp.eu.mailgun.org", 587)
                {
                    Credentials = new NetworkCredential("postmaster@bildirim.nazlisunay.com.tr", "b262ba37e5efc1fbfe2c6df6da8c13ee"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("bildirim@carRental.com", "carRental.com"),
                    Subject = ViewBag.Subject,
                    Body = ViewBag.Body,
                    IsBodyHtml = true
                };

                mailMessage.ReplyToList.Add(new MailAddress(customer.Email));
                mailMessage.To.Add(new MailAddress(customer.Email));

                client.Send(mailMessage);
                return View();
            }
            catch (Exception ex)
            {
                // Hata mesajını loglayın veya kullanıcıya gösterin
                ViewBag.ErrorMessage = ex.Message;
                return View("Error"); // Hata sayfasına yönlendirebilirsiniz
            }
        }



        private Customer GetCurrentCustomer()
        {
            var customerJson = HttpContext.Session.GetString("CurrentCustomer");

            if (string.IsNullOrEmpty(customerJson))
            {
                return null; 
            }

            return JsonConvert.DeserializeObject<Customer>(customerJson);
        }
    }
}
