using Microsoft.EntityFrameworkCore;
using TelegramBot_HoursStore.Models;

namespace TelegramBot_HoursStore
{
    public class DBContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseSqlServer(BotSettings.ConnectionString);

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Seller> Sellers { get; set; }

    }
}
