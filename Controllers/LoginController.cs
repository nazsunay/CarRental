using CarRental.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;

namespace CarRental.Controllers
{
    public class LoginController : Controller
    {

        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=nazlisun_CarRentalDb; User Id=nazlisun_CarRental;Password=Nazli.55?; TrustServerCertificate=True";
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(CustomerLoginModel model)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var customer = connection.QuerySingleOrDefault<Customer>(
                        "SELECT * FROM Customers WHERE Email = @Email AND Password = @Password",
                        new { Email = model.Email, Password = model.Password });

                    if (customer != null)
                    {
                        // Oturum açma işlemi başarılı
                        HttpContext.Session.SetString("CurrentCustomer", JsonConvert.SerializeObject(customer));
                        return Json(new { success = true }); // Başarılı yanıt
                    }
                    else
                    {
                        return Json(new { success = false, message = "Geçersiz e-posta veya şifre." });
                    }
                }
            }

            return Json(new { success = false, message = "Form geçersiz." });
        }

    }
}
