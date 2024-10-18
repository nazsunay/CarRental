using CarRental.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using System.Data.SqlClient;

namespace CarRental.Controllers
{
    public class ContactController : Controller
    {
        

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Contact()
        {
            var model = new ContactViewModel(); // Modeli başlatın veya verileri buraya yükleyin
            return View(model);
        }


        // POST: /Contact/
        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Veriyi veritabanına kaydet
                using (var connection = new SqlConnection(connectionString))
                {
                    var commandText = "INSERT INTO Contact (Name, Email, Message, DateSent) VALUES (@Name, @Email, @Message, @DateSent)";
                    using (var command = new SqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("@Name", model.Name);
                        command.Parameters.AddWithValue("@Email", model.Email);
                        command.Parameters.AddWithValue("@Message", model.Message);
                        command.Parameters.AddWithValue("@DateSent", DateTime.Now);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                // Başarı mesajı
                ViewBag.Message = "Mesajınız başarıyla gönderildi!";
                return View(new ContactViewModel()); // Boş model ile dönüş
            }

            return View(model); // Hatalı model ile dönüş
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
            return RedirectToAction("Index");
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

