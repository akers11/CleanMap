using CleanMap.Models;
using CleanMap.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using BCrypt.Net;

namespace CleanMap.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context; // Контекст базы данных

        public AccountController(ILogger<AccountController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (!_context.Announcements.Any())
            {
                var emptyStatistics = new SiteStatisticsViewModel
                {
                    AnnouncementsCreated = 0,
                    AnnouncementsCompleted = 0,
                    AnnouncementsInProgress = 0,
                    TotalVolunteers = 0,
                    TotalCities = 0,
                    Announcements = new List<object>()
                };

                ViewBag.Message = "Статистика о работе сайта еще не собрана";
                return View(emptyStatistics);
            }

            var announcementsRaw = _context.Announcements
    .Where(a => !string.IsNullOrEmpty(a.GeoLocation) && a.GeoLocation.Contains(","))
    .ToList(); 

            var statistics = new SiteStatisticsViewModel
            {
                AnnouncementsCreated = _context.Announcements.Count(),
                AnnouncementsCompleted = _context.Announcements.Count(a => a.Status == "Выполнено"),
                AnnouncementsInProgress = _context.Announcements.Count(a => a.Status == "В работе"),
                TotalVolunteers = _context.Users.Count(u => u.Role == "Волонтер"),
                TotalCities = _context.Announcements.Select(a => a.City).Distinct().Count(),

                Announcements = announcementsRaw.Select(a =>
                    {
                        var parts = a.GeoLocation.Split(',');
                        if (parts.Length < 2) return null; 

                        if (!decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude) ||
                            !decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                        {
                            return null; 
                        }

                        return new
                        {
                            a.Description,
                            a.Status,
                            a.City,
                            Address = a.City, 
                            Latitude = latitude,
                            Longitude = longitude
                        };

                    })
                    .Where(a => a != null) 
                    .ToList()
            };

            return View(statistics);

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new AccountViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(AccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Пароли не совпадают");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    DateOfBirth = model.DateOfBirth,
                    City = model.City,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    Username = model.Username,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password) // Хеширование пароля
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                await Authenticate(user.Username, user.Role);
                return RedirectToAction("Index", "Account");
            }

            return View(model);
        }


        private async Task Authenticate(string username, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme); 
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal); 
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errorMessage = "Некорректные данные." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                return Json(new { success = false, errorMessage = "Неверный логин или пароль." });
            }

            // Проверка: если превышено количество попыток, блокировка входа на 10 минут
            if (user.FailedLoginAttempts >= 5 && user.LastFailedAttempt.HasValue &&
                user.LastFailedAttempt.Value.AddMinutes(10) > DateTime.UtcNow)
            {
                return Json(new { success = false, errorMessage = "Слишком много попыток входа. Попробуйте через 10 минут." });
            }

            // Проверка пароля
            if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                user.FailedLoginAttempts++; // Увеличение счетчика неудачных попыток
                user.LastFailedAttempt = DateTime.UtcNow;
                _context.SaveChanges();

                return Json(new { success = false, errorMessage = "Неверный логин или пароль." });
            }

            // Сброс счетчика при успешном входе
            user.FailedLoginAttempts = 0;
            user.LastFailedAttempt = null;
            _context.SaveChanges();

            // Авторизация пользователя
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Перенаправление в зависимости от роли
            string redirectUrl = user.Role switch
            {
                "Волонтер" => Url.Action("VolunteerDashBoard", "Volunteer"),
                "Житель" => Url.Action("CitizenDashBoard", "Citizen"),
                _ => Url.Action("Index", "Account")
            };

            return Json(new { success = true, redirectUrl });
        }





        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Выход из системы
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Перенаправление на главную страницу после выхода
            return RedirectToAction("Index", "Account");  // Главное представление
        }


        public IActionResult Statistics()
        {
            if (!_context.Announcements.Any())
            {
                var emptyStatistics = new SiteStatisticsViewModel
                {
                    AnnouncementsCreated = 0,
                    AnnouncementsCompleted = 0,
                    AnnouncementsInProgress = 0,
                    TotalVolunteers = 0,
                    TotalCities = 0,
                    Announcements = new List<object>()
                };

                ViewBag.Message = "Статистика о работе сайта еще не собрана";
                return View(emptyStatistics);
            }

            var announcementsRaw = _context.Announcements
    .Where(a => !string.IsNullOrEmpty(a.GeoLocation) && a.GeoLocation.Contains(","))
    .ToList(); // Загрузка данных в память

            var statistics = new SiteStatisticsViewModel
            {
                AnnouncementsCreated = _context.Announcements.Count(),
                AnnouncementsCompleted = _context.Announcements.Count(a => a.Status == "Выполнено"),
                AnnouncementsInProgress = _context.Announcements.Count(a => a.Status == "В работе"),
                TotalVolunteers = _context.Users.Count(u => u.Role == "Волонтер"),
                TotalCities = _context.Announcements.Select(a => a.City).Distinct().Count(),

                Announcements = announcementsRaw.Select(a =>
                {
                    var parts = a.GeoLocation.Split(',');
                    if (parts.Length < 2) return null; // Пропуск некорректных данных

                    if (!decimal.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude) ||
                        !decimal.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                    {
                        return null; // Пропуск, если координаты некорректны
                    }

                    return new
                    {
                        a.Description,
                        a.Status,
                        a.City,
                        Address = a.City, 
                        Latitude = latitude,
                        Longitude = longitude
                    };

                })
                    .Where(a => a != null) // Удаление элементов, которые были пропущены
                    .ToList()
            };
            
            return View(statistics);
        }

    }
}
