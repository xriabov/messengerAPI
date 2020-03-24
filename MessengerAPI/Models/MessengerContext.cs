using Microsoft.EntityFrameworkCore;

namespace WebApplication.Models
{
    public class MessengerContext: DbContext
    {
        public MessengerContext(DbContextOptions<MessengerContext> options) : base(options)
        {
        }
        
        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}