using CarRental.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Data.SqlClient;

namespace CarRental.Controllers
{
    public class LoginController : Controller
    {

        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=nazlisun_CarRentalDb; User Id=nazlisun_CarRental;Password=Nazli.55?; TrustServerCertificate=True";
        public IActionResult Index()
        {
            return View(new CustomerLoginModel());
        }

        [HttpPost]
        public IActionResult Login(CustomerLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Form geçersiz." });
            }

            var captchaToken = Request.Form["g-recaptcha-response"];

            if (!VerifyCaptcha(captchaToken))
            {
                return Json(new { success = false, message = "reCAPTCHA doğrulaması başarısız." });
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var customer = connection.QuerySingleOrDefault<Customer>(
                    "SELECT * FROM Customers WHERE Email = @Email AND Password = @Password",
                    new { Email = model.Email, Password = model.Password });

                if (customer != null)
                {
                    HttpContext.Session.SetString("CurrentCustomer", JsonConvert.SerializeObject(customer));
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Geçersiz e-posta veya şifre." });
                }
            }
        }

        public bool VerifyCaptcha(string captchaToken)
        {
            var client = new RestClient("https://www.google.com/recaptcha");
            var request = new RestRequest("api/siteverify", Method.Post);
            request.AddParameter("secret", "6LfRHjQqAAAAAJF_s17tp5CLm2Yj9w0pMW10iC44");

            request.AddParameter("response", captchaToken);

            var response = client.Execute<CaptchaResponse>(request);

            if (response.Data.Success && response.Data.Score > 0.5)
            {
                return true;
            }
            return false;
        }

        //[HttpPost]
        //public IActionResult Login(CustomerLoginModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (var connection = new SqlConnection(connectionString))
        //        {

        //            var customer = connection.QuerySingleOrDefault<Customer>(
        //                "SELECT * FROM Customers WHERE Email = @Email AND Password = @Password",
        //                new { Email = model.Email, Password = model.Password });

        //            if (customer != null)
        //            {

        //                HttpContext.Session.SetString("CurrentCustomer", JsonConvert.SerializeObject(customer));
        //                return Json(new { success = true }); // Başarılı yanıt
        //            }
        //            else
        //            {
        //                return Json(new { success = false, message = "Geçersiz e-posta veya şifre." });
        //            }
        //        }
        //    }

        //    return Json(new { success = false, message = "Form geçersiz." });
        //}

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(Customer model)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(connectionString))
                {


                    // Kullanıcının daha önce kayıtlı olup olmadığını kontrol et
                    var existingCustomer = connection.QuerySingleOrDefault<Customer>(
                        "SELECT * FROM Customers WHERE Email = @Email", new { Email = model.Email });

                    if (existingCustomer != null)
                    {
                        ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
                        return View(model);
                    }

                    // Yeni kullanıcıyı ekle
                    var newCustomerId = connection.ExecuteScalar<int>(
                        "INSERT INTO Customers (Name, Email, PhoneNumber, Password) VALUES (@Name, @Email, @PhoneNumber, @Password); SELECT CAST(SCOPE_IDENTITY() as int);",
                        new { Name = model.Name, Email = model.Email, PhoneNumber = model.PhoneNumber, Password = model.Password });

                    // Başarılı kayıt sonrası yönlendirme (örneğin, giriş sayfasına)
                    return RedirectToAction("Index", "Login");
                }
            }

            return View(model);
        }



    }
}
