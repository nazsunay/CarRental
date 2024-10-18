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
       

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string make = "")
        {
            using var connection = new SqlConnection(connectionString);

            
            var carQuery = "SELECT * FROM Cars WHERE Make LIKE @Make";
            var cars = connection.Query<Car>(carQuery, new { Make = $"%{make}%" }).ToList();

            
            var makeQuery = "SELECT DISTINCT Make FROM Cars";
            var makes = connection.Query<string>(makeQuery).ToList();

            
            ViewBag.Models = makes;

            return View(cars); 
        }

        public IActionResult Details(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var car = connection.QuerySingleOrDefault<Car>("SELECT * FROM Cars WHERE Id = @Id", new { Id = id });

                if (car == null)
                {
                    return NotFound();
                }

                return View(car); 
            }
        }
        // Filtreleme 
        [HttpGet]
        public IActionResult Filter(string make)
        {
            using var connection = new SqlConnection(connectionString);
            var query = "SELECT * FROM Cars WHERE Make LIKE @Make";
            var cars = connection.Query<Car>(query, new { Make = $"%{make}%" }).ToList();

           
            var allMakesQuery = "SELECT DISTINCT Make FROM Cars";
            var allMakes = connection.Query<string>(allMakesQuery).ToList();
            ViewBag.Models = allMakes;

            return View("Index", cars); 
        }
    }

}
    

        

