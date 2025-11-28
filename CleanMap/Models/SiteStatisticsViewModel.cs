namespace CleanMap.Models
{
    public class SiteStatisticsViewModel
    {
        
            public int TotalAnnouncements { get; set; }
            public int AnnouncementsCompleted { get; set; }
            public int AnnouncementsInProgress { get; set; }
            public int AnnouncementsCreated { get; set; }
            public int TotalVolunteers { get; set; }
            public int TotalCities { get; set; }

            // Список объявлений для карты
            public IEnumerable<object> Announcements { get; set; }
        

    }

}
