using System;
using System.ComponentModel.DataAnnotations;

namespace CleanMap.Models
{
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        public string? CitizenUsername { get; set; } // Логин пользователя

        public DateTime CreationDate { get; set; } // Дата создания

        public string? Status { get; set; } // "Создано", "В работе", "Выполнено"

        [Required(ErrorMessage = "Город обязателен.")]
        public string City { get; set; } // Город

        [Required(ErrorMessage = "Описание обязательно.")]
        public string Description { get; set; } // Описание

        [Required(ErrorMessage = "Геолокация обязательна.")]
        public string GeoLocation { get; set; } // Координаты

        public string Address { get; set; } // Адрес

        public string? AssignedVolunteer { get; set; } // Логин волонтера
    }
}
