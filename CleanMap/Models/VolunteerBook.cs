namespace CleanMap.Models
{
    public class VolunteerBook
    {
        public int Id { get; set; }
        public string VolunteerId { get; set; } // Логин волонтёра
        public int BookNumber { get; set; } // Номер волонтёрской книжки
    }
}