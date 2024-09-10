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
        [HttpGet]
        public IActionResult Contact()
        {
            return View(new ContactViewModel());
        }

        // POST: /Contact/
        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Mesajı işleme kodunu buraya ekleyin (e-posta gönderimi, veri tabanına kaydetme vb.)

                ViewBag.Message = "Mesajınız başarıyla gönderildi!";
                return View(new ContactViewModel()); // Formu temizlemek için yeni bir model gönderir
            }
            return View(model);
        }
    }
}
