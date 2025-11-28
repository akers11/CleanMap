namespace CleanMap.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; } // Связь с чатом
        public string SenderId { get; set; } // Отправитель (гражданин или волонтёр)
        public string Text { get; set; } // Текст сообщения
        public DateTime SentAt { get; set; } = DateTime.Now; // Время отправки

        public Chat Chat { get; set; }
    }
}
