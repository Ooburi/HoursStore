using System.ComponentModel.DataAnnotations;

namespace TelegramBot_HoursStore.Models
{
    public class Seller
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
    }
}
