using System;
using System.ComponentModel.DataAnnotations;

namespace TelegramBot_HoursStore.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
        public string? CardNumber { get; set; }
        public string Marker { get; set; }//
        public bool Authorized { get; set; } = false;//
        public Guid LastShift { get; set; } //
    }
}
