using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanMap.Models;
using CleanMap.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CleanMap.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получение списка чатов гражданина в JSON
        [HttpGet]
        public async Task<IActionResult> GetCitizenChats()
        {
            string currentUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsername))
            {
                return Unauthorized();
            }

            var chats = await _context.Chats
                .Include(c => c.Announcement)
                .Where(c => c.CitizenId == currentUsername)
                .Select(c => new
                {
                    id = c.Id,
                    announcement = c.Announcement.Description,
                    volunteer = c.VolunteerId,
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Json(chats);
        }

        // Получение списка сообщений по chatId в JSON
        [HttpGet]
        public async Task<IActionResult> GetChatMessages(int chatId)
        {
            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    sender = m.SenderId,
                    text = m.Text,
                    sentAt = m.SentAt
                })
                .ToListAsync();

            return Json(messages);
        }

        // Отправка сообщения через AJAX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            Console.WriteLine($"Получен запрос на отправку сообщения: ChatId={request?.ChatId}, Text={request?.Text}");

            if (request == null || string.IsNullOrWhiteSpace(request.Text))
            {
                Console.WriteLine("Ошибка: пустое сообщение");
                return BadRequest(new { error = "Сообщение не может быть пустым." });
            }

            var chat = await _context.Chats.FindAsync(request.ChatId);
            if (chat == null)
            {
                Console.WriteLine("Ошибка: Чат не найден");
                return NotFound(new { error = "Чат не найден." });
            }

            var message = new Message
            {
                ChatId = request.ChatId,
                SenderId = User.Identity?.Name,
                Text = request.Text,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            Console.WriteLine("Сообщение успешно сохранено!");
            return Ok(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetVolunteerChats()
        {
            string currentUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsername))
            {
                return Unauthorized();
            }

            var chats = await _context.Chats
                .Include(c => c.Announcement)
                .Where(c => c.VolunteerId == currentUsername && c.Announcement.Status == "В работе") // Фильтр по статусу объявления
                .Select(c => new
                {
                    id = c.Id,
                    announcement = c.Announcement.Description,
                    citizen = c.CitizenId,
                    createdAt = c.CreatedAt
                })
                .ToListAsync();

            return Json(chats);
        }



        public class SendMessageRequest
        {
            public int ChatId { get; set; }
            public string Text { get; set; }
        }


        public class MessageDto
        {
            public int ChatId { get; set; }
            public string Text { get; set; }
        }
    }
}
