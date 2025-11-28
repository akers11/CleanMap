namespace CleanMap.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public int AnnouncementId { get; set; } // Связь с объявлением
        public string VolunteerId { get; set; } // ID волонтёра
        public string CitizenId { get; set; } // ID гражданина
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Дата создания

        public Announcement Announcement { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
