using CarRental.Models;
using Microsoft.AspNetCore.Mvc;

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
                // Veriyi işleme veya göndermeyi yapın
                ViewBag.Message = "Mesajınız başarıyla gönderildi!";
                return View(new ContactViewModel()); // Boş model veya yeni veri ile dönün
            }
            return View(model); // Modeli geri dönün
        }

    }
}
