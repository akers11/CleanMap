using CleanMap.Models;
using CleanMap.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace CleanMap.Controllers
{
    public class CitizenController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitizenController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Панель объявлений
        public IActionResult CitizenDashboard()
        {
            string currentUser = User.Identity.Name; // Получение текущего пользователя
            var announcements = _context.Announcements
                .Where(a => a.CitizenUsername == currentUser)
                .ToList();

            return View(announcements);
        }

        [HttpGet]
        public IActionResult CreateAnnouncement()
        {
            return View(new Announcement());
        }

        // Создание объявления
        [HttpPost]
        public IActionResult CreateAnnouncement(Announcement model)
        {
            model.CreationDate = DateTime.Now;
            model.Status = "Создано";
            model.CitizenUsername = User.Identity?.Name ?? "Unknown";
            model.AssignedVolunteer = "Не назначен";

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Ошибки валидации:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(model);
            }

            try
            {
                _context.Announcements.Add(model);
                _context.SaveChanges();
                Console.WriteLine("Объявление успешно сохранено.");
                return RedirectToAction("CitizenDashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
                ModelState.AddModelError("", "Ошибка сохранения объявления.");
            }

            return View(model);
        }


        // Подробная информация
        public IActionResult AnnouncementDetails(int id)
        {
            var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(); // Завершение сессии
            return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
        }



    }
}
