using CarRental.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IO;

namespace CarRental.Controllers
{
    public class AdminController : Controller
    {
        
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Car>("SELECT * FROM Cars").ToList();

            return View(posts);
        }

        public IActionResult Add()
        {
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<Car>("SELECT * FROM Cars").ToList();

            return View(posts);
        }

        [HttpPost]
        public IActionResult Add(Car model, IFormFile Image)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptınız.";
                return View(model); 
            }

            using var connection = new SqlConnection(connectionString);

            var ilanlar = "INSERT INTO Cars (Make, Model, Year, ImgUrl, DailyRate, IsAvailable) VALUES (@Make, @Model, @Year, @ImgUrl, @DailyRate, @IsAvailable)";

            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
            using var stream = new FileStream(path, FileMode.Create);
            Image.CopyTo(stream);
            model.ImgUrl = imageName;

            var data = new
            {
                model.Make,
                model.Model,
                model.Year,
                model.ImgUrl,
                model.DailyRate,
                model.IsAvailable
            };

            var rowsAffected = connection.Execute(ilanlar, data);

            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Araç başarıyla eklendi.";
            return View("Message");  
        }

        public IActionResult Edit(int id)
        {

            using var connection = new SqlConnection(connectionString);
            var post = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = id });

            return View(post);
        }

        [HttpPost]
        public IActionResult Edit(Car model, IFormFile Image)
        {

            using var connection = new SqlConnection(connectionString);
            var imageName = model.ImgUrl;
            if (Image != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads", imageName);
                using var stream = new FileStream(path, FileMode.Create);
                Image.CopyTo(stream);
            }

            var sql = "UPDATE Cars SET Make = @Make, Model = @Model, Year = @Year, ImgUrl = @ImgUrl, DailyRate = @DailyRate, IsAvailable = @IsAvailable WHERE Id = @Id";

            var param = new
            {
                model.Make,
                Model = model.Model,
                Year = model.Year,
                DailyRate = model.DailyRate,
                IsAvailable = model.IsAvailable,
                Id = model.Id,
                ImgUrl = imageName,
            };


            var affectedRows = connection.Execute(sql, param);


            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }

        public IActionResult Delete(int id)
        {

            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM Cars WHERE Id=@Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index"); ;
        }

    }
}
