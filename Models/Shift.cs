using System;
using System.ComponentModel.DataAnnotations;

namespace TelegramBot_HoursStore.Models
{
    public class Shift
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string TimeSlot { get; set; }
        public bool Active { get; set; } = false;
        public int HoursNumber { get; set; }
        public double Price { get; set; }
        public int? SellerId { get; set; }
        public User? Seller { get; set; }
        public int? BuyerId { get; set; }
        public User? Buyer { get; set; }
        public DateTime? ActivetedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public Guid Guid { get; set; } = Guid.Empty;
    }
}
