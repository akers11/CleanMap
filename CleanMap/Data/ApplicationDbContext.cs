using CleanMap.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CleanMap.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Announcement> Announcements { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; } 
        public DbSet<Message> Messages { get; set; } 

        public DbSet<VolunteerBook> VolunteerBooks { get; set; }
        public DbSet<VolunteerAnnouncement> VolunteerAnnouncements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VolunteerAnnouncement>()
                .HasOne(va => va.VolunteerBook)
                .WithMany()
                .HasForeignKey(va => va.VolunteerBookId)
                .OnDelete(DeleteBehavior.Restrict); // Защита от удаления связанных записей
        }
    }
}
