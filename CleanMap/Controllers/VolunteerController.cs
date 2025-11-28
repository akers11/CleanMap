using CleanMap.Models;
using CleanMap.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace CleanMap.Controllers
{
    public class VolunteerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Главная панель волонтёра
        public IActionResult VolunteerDashboard()
        {
            var announcements = _context.Announcements
                .Where(a => a.Status == "Создано" && a.AssignedVolunteer == "Не назначен") 
                .ToList();

            if (!announcements.Any())
            {
                Console.WriteLine("Нет доступных объявлений для отображения.");
            }

            return View(announcements);
        }


        // Принятые объявления
        public IActionResult AcceptedAnnouncements()
        {
            string currentUser = User.Identity?.Name; // Авторизованный волонтёр

            if (string.IsNullOrEmpty(currentUser))
            {
                Console.WriteLine("Ошибка: Пользователь не авторизован.");
                return RedirectToAction("Login", "Account");
            }

            var announcements = _context.Announcements
                .Where(a => a.AssignedVolunteer == currentUser) // Фильтр по текущему пользователю
                .ToList();

            if (!announcements.Any())
            {
                Console.WriteLine($"Нет принятых объявлений для пользователя {currentUser}.");
            }

            return View("AcceptedAnnouncements", announcements); // Возврат нового представления
        }



        // Подробности объявления
        [HttpGet]
        public IActionResult AnnouncementDetails(int id)
        {
            var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View("~/Views/Volunteer/AnnouncementDetails.cshtml", announcement);
        }


        // Принятие объявления в работу
        [HttpPost]
        public IActionResult AcceptAnnouncement(int id)
        {
            var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
            string currentUser = User.Identity?.Name;

            // Проверка наличия у пользователя волонтерской книжки
            var volunteerBook = _context.VolunteerBooks.FirstOrDefault(vb => vb.VolunteerId == currentUser);
            if (volunteerBook == null)
            {
                TempData["ErrorMessage"] = "У вас отсутствует волонтерская книжка. Пройдите в раздел портфолио.";
                return RedirectToAction("VolunteerDashboard");
            }
            else
            {
                // Обновление статуса и назначение волонтёра
                announcement.Status = "В работе";
                announcement.AssignedVolunteer = currentUser;

                // Запись в историю
                var volunteerAnnouncement = new VolunteerAnnouncement
                {
                    VolunteerId = currentUser,
                    AnnouncementId = announcement.Id,
                    AcceptedAt = DateTime.Now,
                    VolunteerBookId = volunteerBook.Id // Указание ID книжки
                };

                _context.VolunteerAnnouncements.Add(volunteerAnnouncement);

                // Проверка наличия чата
                var existingChat = _context.Chats
                    .FirstOrDefault(c => c.AnnouncementId == announcement.Id
                                         && c.CitizenId == announcement.CitizenUsername
                                         && c.VolunteerId == currentUser);

                if (existingChat == null)
                {
                    var chat = new Chat
                    {
                        AnnouncementId = announcement.Id,
                        CitizenId = announcement.CitizenUsername,
                        VolunteerId = currentUser
                    };
                    _context.Chats.Add(chat);
                }

                try
                {
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Объявление успешно принято в работу!";
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"DbUpdateException: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = "Ошибка при принятии объявления.";
                }
                return RedirectToAction("VolunteerDashboard");
            }
                        
        }

        public IActionResult Portfolio()
        {
            string currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return RedirectToAction("Login", "Account");
            }

            // Получение номера волонтёрской книжки
            var volunteerBook = _context.VolunteerBooks
                .FirstOrDefault(vb => vb.VolunteerId == currentUser);

            ViewBag.VolunteerBook = volunteerBook?.BookNumber;

            // Получение списка всех объявлений волонтёра
            var announcements = _context.VolunteerAnnouncements
                .Where(va => va.VolunteerBook.VolunteerId == currentUser)
                .ToList() // Принудительная загрузка данных в память
                .Select(va => new
                {
                    va.AnnouncementId,
                    va.AcceptedAt,
                    va.CompletedAt,
                    Status = va.CompletedAt == null ? "В работе" : "Завершено",
                    HoursSpent = va.CompletedAt == null ? "—" :
                                 $"{(va.CompletedAt - va.AcceptedAt)?.TotalHours:F2} ч"
                })
                .ToList();

            ViewBag.CompletedAnnouncements = announcements;

            return View();
        }



        [HttpPost]
        public IActionResult AddVolunteerBook(int bookNumber)
        {
            string currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                TempData["ErrorMessage"] = "Вы не авторизованы.";
                return RedirectToAction("Login", "Account");
            }

            // Проверка наличия записи в базе
            var existingBook = _context.VolunteerBooks.FirstOrDefault(vb => vb.VolunteerId == currentUser);
            if (existingBook != null)
            {
                TempData["ErrorMessage"] = "Волонтерская книжка уже добавлена.";
                return RedirectToAction("VolunteerDashboard");
            }

            // Добавление записи
            var newBook = new VolunteerBook
            {
                VolunteerId = currentUser,
                BookNumber = bookNumber
            };
            _context.VolunteerBooks.Add(newBook);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Волонтерская книжка успешно добавлена!";
            return RedirectToAction("VolunteerDashboard");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsCompleted(int id)
        {
            var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
            string currentUser = User.Identity?.Name;
            var volunteerRecord = _context.VolunteerAnnouncements
                .FirstOrDefault(v => v.AnnouncementId == id && v.VolunteerBook.VolunteerId == currentUser);

            // Фиксирование даты завершения
            volunteerRecord.CompletedAt = DateTime.Now;

            // Подсчет времени работы
            var hoursSpent = (volunteerRecord.CompletedAt - volunteerRecord.AcceptedAt)?.TotalHours ?? 0;

            // Смена статуса объявления
            announcement.Status = "Выполнено";

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Объявление завершено! Время работы: {hoursSpent:F2} часов.";
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                TempData["ErrorMessage"] = "Ошибка при завершении объявления.";
            }

            return RedirectToAction("VolunteerDashboard");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeclineAnnouncement([FromBody] int id)
        {
            try
            {
                var announcement = _context.Announcements.FirstOrDefault(a => a.Id == id);
                if (announcement == null)
                {
                    return NotFound(new { message = "Объявление не найдено." });
                }

                string currentUser = User.Identity?.Name;

                var volunteerRecord = _context.VolunteerAnnouncements
                    .FirstOrDefault(v => v.AnnouncementId == id && v.VolunteerId == currentUser);

                if (volunteerRecord != null)
                {
                    _context.VolunteerAnnouncements.Remove(volunteerRecord);
                }
                // Поиск чата, связанного с этим объявлением
                var chat = _context.Chats.FirstOrDefault(c => c.AnnouncementId == id);
                if (chat != null)
                {
                    // Удаление всех сообщений, связанных с этим чатом
                    var messages = _context.Messages.Where(m => m.ChatId == chat.Id);
                    _context.Messages.RemoveRange(messages);

                    // Удаление чата
                    _context.Chats.Remove(chat);
                }
                announcement.Status = "Создано";
                announcement.AssignedVolunteer = "Не назначен";

                _context.SaveChanges();

                return Ok(new { message = "Вы отказались от объявления." });
            }
            catch
            {
                return StatusCode(500, new { message = "Ошибка при отказе от объявления." });
            }
        }



        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(); // Завершение сессии
            return RedirectToAction("Index", "Home"); // Перенаправление на главную страницу
        }

    }
}