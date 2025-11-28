namespace CleanMap.Models
{
    public class VolunteerAnnouncement
    {
        public int Id { get; set; } // Первичный ключ
        public string VolunteerId { get; set; }
        public int AnnouncementId { get; set; }
        public DateTime AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int VolunteerBookId { get; set; }
        public VolunteerBook VolunteerBook { get; set; }

    }
}