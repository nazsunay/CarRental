using CarRental.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace CarRental.Controllers
{
    public class AdminController : Controller
    {
        string connectionString = "Server=104.247.162.242\\MSSQLSERVER2019;Initial Catalog=nazlisun_CarRentalDb; User Id=nazlisun_CarRental;Password=Nazli.55?; TrustServerCertificate=True";
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Car>("SELECT * FROM Cars").ToList();

            return View(posts);
        }

        [HttpPost]
        public IActionResult Add(Car model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    msg = "Eksik veya hatali bilgi girisi yaptin.."
                });
            }

            using var connection = new SqlConnection(connectionString);

            var newRecordId = connection.ExecuteScalar<int>("INSERT INTO Cars (Make, Model, Year, ImgUrl, DailyRate, IsAvailable) VALUES (@Make, @Model, @Year, @ImgUrl, @DailyRate, @IsAvailable) SELECT SCOPE_IDENTITY()", model);
            model.Id = newRecordId;

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(Car model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    msg = "Eksik veya hatali bilgi girisi yaptin.."
                });
            }

            using var connection = new SqlConnection(connectionString);

            var newRecordId = connection.ExecuteScalar<int>("UPDATE Cars SET Name=@Name, Make=@Make, Model=@Model,Year=@Year, ImgUrl=@ImgUrl, DailyRate=@DailyRate, IsAvailable=@IsAvailable WHERE Id = @Id SELECT SCOPE_IDENTITY()", model);
            model.Id = newRecordId;

            return View(model);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    msg = "Eksik veya hatali bilgi girisi yaptin.."
                });
            }

            using var connection = new SqlConnection(connectionString);

            var newRecordId = connection.ExecuteScalar<int>("DELETE FROM Cars WHERE Id = @Id SELECT SCOPE_IDENTITY()", new { Id = id });

            return View(new
            {
                msg = "Silme islemi basariyla gerceklesti"
            });
        }
    }
}
