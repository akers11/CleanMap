using System.ComponentModel.DataAnnotations;

namespace CleanMap.Models
{
    public class User
    {
        [Key]
        public string Username { get; set; } 
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedAttempt { get; set; }
    }

}
